using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.UI.ViewModels;
using JuliusSweetland.OptiKey.UI.Windows;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Appender; //Do not remove even if marked as unused by Resharper - it is used by the Release build configuration
using NBug.Core.UI; //Do not remove even if marked as unused by Resharper - it is used by the Release build configuration
using Octokit;
using presage;
using Application = System.Windows.Application;
using JuliusSweetland.OptiKey.Services.PluginEngine;

namespace JuliusSweetland.OptiKey
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constants

        private const string GazeTrackerUdpRegex = @"^STREAM_DATA\s(?<instanceTime>\d+)\s(?<x>-?\d+(\.[0-9]+)?)\s(?<y>-?\d+(\.[0-9]+)?)";
        private const string GitHubRepoName = "optikey";
        private const string GitHubRepoOwner = "optikey";
        private const string ExpectedMaryTTSLocationSuffix = @"\bin\marytts-server.bat";

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Action applyTheme;

        #endregion

        #region Ctor

        public App()
        {
            //Setup unhandled exception handling and NBug
            AttachUnhandledExceptionHandlers();

            //Log startup diagnostic info
            Log.Info("STARTING UP.");
            LogDiagnosticInfo();

            //Attach shutdown handler
            Current.Exit += (o, args) =>
            {
                Log.Info("PERSISTING USER SETTINGS AND SHUTTING DOWN.");
                Settings.Default.Save();
            };

            HandleCorruptSettings();

            //Upgrade settings (if required) - this ensures that user settings migrate between version changes
            if (Settings.Default.SettingsUpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgradeRequired = false;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            //Adjust log4net logging level if in debug mode
            ((Hierarchy)LogManager.GetRepository()).Root.Level = Settings.Default.Debug ? Level.Debug : Level.Info;
            ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);

            //Apply resource language (and listen for changes)
            Action<Languages> applyResourceLanguage = language => OptiKey.Properties.Resources.Culture = language.ToCultureInfo();
            Settings.Default.OnPropertyChanges(s => s.UiLanguage).Subscribe(applyResourceLanguage);
            applyResourceLanguage(Settings.Default.UiLanguage);

            //Logic to initially apply the theme and change the theme on setting changes
            applyTheme = () =>
            {
                var themeDictionary = new ThemeResourceDictionary
                {
                    Source = new Uri(Settings.Default.Theme, UriKind.Relative)
                };

                var previousThemes = Resources.MergedDictionaries
                    .OfType<ThemeResourceDictionary>()
                    .ToList();

                //N.B. Add replacement before removing the previous as having no applicable resource
                //dictionary can result in the first element not being rendered (usually the first key).
                Resources.MergedDictionaries.Add(themeDictionary);
                previousThemes.ForEach(rd => Resources.MergedDictionaries.Remove(rd));
            };

            Settings.Default.OnPropertyChanges(settings => settings.Theme).Subscribe(_ => applyTheme());
        }

        #endregion

        #region On Startup

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Log.Info("Boot strapping the services and UI.");

                //Apply theme
                applyTheme();

                //Define MainViewModel before services so I can setup a delegate to call into the MainViewModel
                //This is to work around the fact that the MainViewModel is created after the services.
                MainViewModel mainViewModel = null;
                Action<KeyValue> fireKeySelectionEvent = kv =>
                {
                    if (mainViewModel != null) //Access to modified closure is a good thing here, for once!
                    {
                        mainViewModel.FireKeySelectionEvent(kv);
                    }
                };

                CleanupAndPrepareCommuniKateInitialState();

                ValidateDynamicKeyboardLocation();

                // Handle plugins. Validate if directory exists and is accessible and pre-load all plugins, building a in-memory list of available ones.
                ValidatePluginsLocation();
                PluginEngine.LoadAvailablePlugins();

                var presageInstallationProblem = PresageInstallationProblemsDetected();

                //Create services
                var errorNotifyingServices = new List<INotifyErrors>();
                IAudioService audioService = new AudioService();
                
                IDictionaryService dictionaryService = new DictionaryService(Settings.Default.SuggestionMethod);
                IPublishService publishService = new PublishService();
                ISuggestionStateService suggestionService = new SuggestionStateService();
                ICalibrationService calibrationService = CreateCalibrationService();
                ICapturingStateManager capturingStateManager = new CapturingStateManager(audioService);
                ILastMouseActionStateManager lastMouseActionStateManager = new LastMouseActionStateManager();
                IKeyStateService keyStateService = new KeyStateService(suggestionService, capturingStateManager, lastMouseActionStateManager, calibrationService, fireKeySelectionEvent);
                IInputService inputService = CreateInputService(keyStateService, dictionaryService, audioService, calibrationService, capturingStateManager, errorNotifyingServices);
                IKeyboardOutputService keyboardOutputService = new KeyboardOutputService(keyStateService, suggestionService, publishService, dictionaryService, fireKeySelectionEvent);
                IMouseOutputService mouseOutputService = new MouseOutputService(publishService);
                errorNotifyingServices.Add(audioService);
                errorNotifyingServices.Add(dictionaryService);
                errorNotifyingServices.Add(publishService);
                errorNotifyingServices.Add(inputService);

                ReleaseKeysOnApplicationExit(keyStateService, publishService);

                //Compose UI
                var mainWindow = new MainWindow(audioService, dictionaryService, inputService, keyStateService);
                IWindowManipulationService mainWindowManipulationService = CreateMainWindowManipulationService(mainWindow);
                errorNotifyingServices.Add(mainWindowManipulationService);
                mainWindow.WindowManipulationService = mainWindowManipulationService;

				//Subscribing to the on closing events.
				mainWindow.Closing += dictionaryService.OnAppClosing;

				mainViewModel = new MainViewModel(
                    audioService, calibrationService, dictionaryService, keyStateService,
                    suggestionService, capturingStateManager, lastMouseActionStateManager,
                    inputService, keyboardOutputService, mouseOutputService, mainWindowManipulationService, errorNotifyingServices);

                mainWindow.MainView.DataContext = mainViewModel;

                //Setup actions to take once main view is loaded (i.e. the view is ready, so hook up the services which kicks everything off)
                Action postMainViewLoaded = () =>
                {
                    mainViewModel.AttachErrorNotifyingServiceHandlers();
                    mainViewModel.AttachInputServiceEventHandlers();
                };

                if(mainWindow.MainView.IsLoaded)
                {
                    postMainViewLoaded();
                }
                else
                {
                    RoutedEventHandler loadedHandler = null;
                    loadedHandler = (s, a) =>
                    {
                        postMainViewLoaded();
                        mainWindow.MainView.Loaded -= loadedHandler; //Ensure this handler only triggers once
                    };
					
                    mainWindow.MainView.Loaded += loadedHandler;
                }

                //Show the main window
                mainWindow.Show();

                if (Settings.Default.LookToScrollEnabled && Settings.Default.LookToScrollShowOverlayWindow)
                {
                    // Create the overlay window, but don't show it yet. It'll make itself visible when the conditions are right.
                    new LookToScrollOverlayWindow(mainViewModel);
                }
                
                //Display splash screen and check for updates (and display message) after the window has been sized and positioned for the 1st time
                EventHandler sizeAndPositionInitialised = null;
                sizeAndPositionInitialised = async (_, __) =>
                {
                    mainWindowManipulationService.SizeAndPositionInitialised -= sizeAndPositionInitialised; //Ensure this handler only triggers once
                    await ShowSplashScreen(inputService, audioService, mainViewModel);
                    await mainViewModel.RaiseAnyPendingErrorToastNotifications();
                    await AttemptToStartMaryTTSService(inputService, audioService, mainViewModel);
                    await AlertIfPresageBitnessOrBootstrapOrVersionFailure(presageInstallationProblem, inputService, audioService, mainViewModel);

                    inputService.RequestResume(); //Start the input service

                    await CheckForUpdates(inputService, audioService, mainViewModel);
                };

                if (mainWindowManipulationService.SizeAndPositionIsInitialised)
                {
                    sizeAndPositionInitialised(null, null);
                }
                else
                {
                    mainWindowManipulationService.SizeAndPositionInitialised += sizeAndPositionInitialised;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting up application", ex);
                throw;
            }
        }

        private static bool PresageInstallationProblemsDetected()
        {
            if (Settings.Default.SuggestionMethod == SuggestionMethods.Presage)
            {
                //1.Check the install path to detect if the wrong bitness of Presage is installed
                string presagePath = null;
                string presageStartMenuFolder = null;
                string osBitness = DiagnosticInfo.OperatingSystemBitness;

                try
                {
                    presagePath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Presage", "", string.Empty).ToString();
                    presageStartMenuFolder = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Presage", "Start Menu Folder", string.Empty).ToString();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Caught exception: {0}", ex);
                }

                Log.InfoFormat("Presage path: {0} | Presage start menu folder: {1} | OS bitness: {2}", presagePath, presageStartMenuFolder, osBitness);

                if (string.IsNullOrEmpty(presagePath)
                    || string.IsNullOrEmpty(presageStartMenuFolder))
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Log.Error("Invalid Presage installation detected (path(s) missing). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                    return true;
                }

                if (presageStartMenuFolder != "presage-0.9.2~beta20150909"
                    && presageStartMenuFolder != "presage-0.9.1")
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Log.Error("Invalid Presage installation detected (valid version not detected). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                    return true;
                }

                if ((osBitness == "64-Bit" && presagePath != @"C:\Program Files (x86)\presage")
                    || (osBitness == "32-Bit" && presagePath != @"C:\Program Files\presage"))
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Log.Error("Invalid Presage installation detected (incorrect bitness? Install location is suspect). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                    return true;
                }

                if (!Directory.Exists(presagePath))
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Log.Error("Invalid Presage installation detected (install directory does not exist). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                    return true;
                }

                //2.Attempt to construct a Presage object, which can fail for a few reasons, including BadImageFormatExceptions (64-bit version installed)
                Presage presageTestInstance = null;
                try
                {
                    //Test Presage constructor call for DllNotFoundException or BadImageFormatException
                    presageTestInstance = new Presage(() => "", () => "");
                }
                catch (Exception ex)
                {
                    //If the installed version of Presage is the wrong format (i.e. 64 bit) then a BadImageFormatException can occur.
                    //If Presage has been deleted, corrupted, or another problem, then a DllLoadException can occur.
                    //These causes an additional problem as the Presage object will probably be non-deterministically
                    //finalised, which will cause this exception again and crash OptiKey. The workaround is to suppress finalisation 
                    //if an object is available (which it generally won't be!), or to warn the user and react.

                    //Set the suggestion method to NGram so that the IDictionaryService can be instantiated without crashing OptiKey
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Settings.Default.Save(); //Save as OptiKey can crash as soon as the finaliser is called by the GC
                    Log.Error("Presage failed to bootstrap - attempting to suppress finalisation. The suggestion method has been changed to NGram", ex);

                    //Attempt to suppress finalisation on the presage instance, or just warn the user
                    if (presageTestInstance != null)
                    {
                        GC.SuppressFinalize(presageTestInstance);
                    }
                    else
                    {
                        MessageBox.Show(
                            OptiKey.Properties.Resources.PRESAGE_CONSTRUCTOR_EXCEPTION_MESSAGE,
                            OptiKey.Properties.Resources.PRESAGE_CONSTRUCTOR_EXCEPTION_TITLE,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    return true;
                }
                // set the config
                if (presageTestInstance != null)
                {
                    if (File.Exists(Settings.Default.PresageDatabaseLocation))
                        presageTestInstance.set_config("Presage.Predictors.DefaultSmoothedNgramPredictor.DBFILENAME", Path.GetFullPath(Settings.Default.PresageDatabaseLocation));
                    else 
                    {
                        string ApplicationDataSubPath = @"JuliusSweetland\OptiKey";
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
                        var database = Path.Combine(path, @"Presage\database.db");
                        if (!File.Exists(database))
                        {
                            try
                            {
                                using (ZipArchive archive = ZipFile.Open(@".\Resources\Presage\database.zip", ZipArchiveMode.Read))
                                {
                                    archive.ExtractToDirectory(path);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.ErrorFormat("Unpacking the Presage database failed with the following exception: \n{0}", e);
                            }
                        }
                        Settings.Default.PresageDatabaseLocation = database;
                        presageTestInstance.set_config("Presage.Predictors.DefaultSmoothedNgramPredictor.DBFILENAME", Path.GetFullPath(Settings.Default.PresageDatabaseLocation));
                    }
                    presageTestInstance.set_config("Presage.Selector.REPEAT_SUGGESTIONS", "yes");
                    presageTestInstance.set_config("Presage.Selector.SUGGESTIONS", Settings.Default.PresageNumberOfSuggestions.ToString());
                    presageTestInstance.save_config();
                    Log.Info("Presage settings set successfully.");
                }
            }
            
            return false;
        }

        #endregion

		#region Create Main Window Manipulation Service

		    private WindowManipulationService CreateMainWindowManipulationService(MainWindow mainWindow)
        {
            return new WindowManipulationService(
                mainWindow,
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowOpacity from settings with value '{0}'", Settings.Default.MainWindowOpacity);
                    }
                    return Settings.Default.MainWindowOpacity;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowState from settings with value '{0}'", Settings.Default.MainWindowState);
                    }
                    return Settings.Default.MainWindowState;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowPreviousState from settings with value '{0}'", Settings.Default.MainWindowPreviousState);
                    }
                    return Settings.Default.MainWindowPreviousState;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowFloatingSizeAndPosition from settings with value '{0}'", Settings.Default.MainWindowFloatingSizeAndPosition);
                    }
                    return Settings.Default.MainWindowFloatingSizeAndPosition;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowDockPosition from settings with value '{0}'", Settings.Default.MainWindowDockPosition);
                    }
                    return Settings.Default.MainWindowDockPosition;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowDockSize from settings with value '{0}'", Settings.Default.MainWindowDockSize);
                    }
                    return Settings.Default.MainWindowDockSize;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowFullDockThicknessAsPercentageOfScreen from settings with value '{0}'", Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen);
                    }
                    return Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness from settings with value '{0}'", Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness);
                    }
                    return Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting MainWindowMinimisedPosition from settings with value '{0}'", Settings.Default.MainWindowMinimisedPosition);
                    }
                    return Settings.Default.MainWindowMinimisedPosition;
                },
                o =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowOpacity to settings with value '{0}'", o);
                    }
                    Settings.Default.MainWindowOpacity = o;
                },
                state =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowState to settings with value '{0}'", state);
                    }
                    Settings.Default.MainWindowState = state;
                },
                state =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowPreviousState to settings with value '{0}'", state);
                    }
                    Settings.Default.MainWindowPreviousState = state;
                },
                rect =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowFloatingSizeAndPosition to settings with value '{0}'", rect);
                    }
                    Settings.Default.MainWindowFloatingSizeAndPosition = rect;
                },
                pos =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowDockPosition to settings with value '{0}'", pos);
                    }
                    Settings.Default.MainWindowDockPosition = pos;
                },
                size =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowDockSize to settings with value '{0}'", size);
                    }
                    Settings.Default.MainWindowDockSize = size;
                },
                t =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowFullDockThicknessAsPercentageOfScreen to settings with value '{0}'", t);
                    }
                    Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = t;
                },
                t =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Storing MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness to settings with value '{0}'", t);
                    }
                    Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness = t;
                });
        }

        #endregion

        #region Attach Unhandled Exception Handlers

        private static void AttachUnhandledExceptionHandlers()
        {
            Current.DispatcherUnhandledException += (sender, args) => Log.Error("A DispatcherUnhandledException has been encountered...", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error("An UnhandledException has been encountered...", args.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) => Log.Error("An UnobservedTaskException has been encountered...", args.Exception);

#if !DEBUG
            Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            TaskScheduler.UnobservedTaskException += NBug.Handler.UnobservedTaskException;

            NBug.Settings.ProcessingException += (exception, report) =>
            {
                //Add latest log file contents as custom info in the error report
                var rootAppender = ((Hierarchy)LogManager.GetRepository())
                    .Root.Appenders.OfType<FileAppender>()
                    .FirstOrDefault();

                if (rootAppender != null)
                {
                    using (var fs = new FileStream(rootAppender.File, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var sr = new StreamReader(fs, Encoding.Default))
                        {
                            var logFileText = sr.ReadToEnd();
                            report.CustomInfo = logFileText;
                        }
                    }
                }
            };

            NBug.Settings.CustomUIEvent += (sender, args) =>
            {
                var crashWindow = new CrashWindow
                {
                    Topmost = true,
                    ShowActivated = true
                };
                crashWindow.ShowDialog();

                //The crash report has not been created yet - the UIDialogResult SendReport param determines what happens next
                args.Result = new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send);
            };

            NBug.Settings.InternalLogWritten += (logMessage, category) => Log.DebugFormat("NBUG:{0} - {1}", category, logMessage);
#endif
        }

        #endregion

        #region Handle Corrupt Settings

        private static void HandleCorruptSettings()
        {
            try
            {
                //Attempting to read a setting from a corrupt user config file throws an exception
                var upgradeRequired = Settings.Default.SettingsUpgradeRequired;
            }
            catch (ConfigurationErrorsException cee)
            {
                Log.Warn("User settings file is corrupt and needs to be corrected. Alerting user and shutting down.");
                string filename = ((ConfigurationErrorsException)cee.InnerException).Filename;

                if (MessageBox.Show(
                        OptiKey.Properties.Resources.CORRUPTED_SETTINGS_MESSAGE,
                        OptiKey.Properties.Resources.CORRUPTED_SETTINGS_TITLE,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    File.Delete(filename);
                    try
                    {
                        System.Windows.Forms.Application.Restart();
                    }
                    catch {} //Swallow any exceptions (e.g. DispatcherExceptions) - we're shutting down so it doesn't matter.
                }
                Current.Shutdown(); //Avoid the inevitable crash by shutting down gracefully
            }
        }

        #endregion

        #region Create Service Methods

        private static ICalibrationService CreateCalibrationService()
        {
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.TheEyeTribe:
                    return new TheEyeTribeCalibrationService();

                case PointsSources.Alienware17:
                case PointsSources.SteelseriesSentry:
                case PointsSources.TobiiEyeX:
                case PointsSources.TobiiEyeTracker4C:
                case PointsSources.TobiiRex:
                case PointsSources.TobiiPcEyeGo:
                case PointsSources.TobiiPcEyeGoPlus:
                case PointsSources.TobiiPcEyeMini:
                case PointsSources.TobiiX2_30:
                case PointsSources.TobiiX2_60:
                    return new TobiiEyeXCalibrationService();

                case PointsSources.VisualInteractionMyGaze:
                    return new MyGazeCalibrationService();
            }

            return null;
        }

        private static IInputService CreateInputService(
            IKeyStateService keyStateService,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            ICalibrationService calibrationService,
            ICapturingStateManager capturingStateManager,
            List<INotifyErrors> errorNotifyingServices)
        {
            Log.Info("Creating InputService.");

            //Instantiate point source
            IPointSource pointSource;
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.GazeTracker:
                    pointSource = new GazeTrackerSource(
                        Settings.Default.PointTtl,
                        Settings.Default.GazeTrackerUdpPort,
                        new Regex(GazeTrackerUdpRegex));
                    break;

                case PointsSources.MousePosition:
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);
                    break;

                case PointsSources.TheEyeTribe:
                    var theEyeTribePointService = new TheEyeTribePointService();
                    errorNotifyingServices.Add(theEyeTribePointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        theEyeTribePointService);
                    break;

                case PointsSources.Alienware17:
                case PointsSources.SteelseriesSentry:
                case PointsSources.TobiiEyeX:
                case PointsSources.TobiiEyeTracker4C:
                case PointsSources.TobiiRex:
                case PointsSources.TobiiPcEyeGo:
                case PointsSources.TobiiPcEyeGoPlus:
                case PointsSources.TobiiPcEyeMini:
                case PointsSources.TobiiX2_30:
                case PointsSources.TobiiX2_60:
                    var tobiiEyeXPointService = new TobiiEyeXPointService();
                    var tobiiEyeXCalibrationService = calibrationService as TobiiEyeXCalibrationService;
                    if (tobiiEyeXCalibrationService != null)
                    {
                        tobiiEyeXCalibrationService.EyeXHost = tobiiEyeXPointService.EyeXHost;
                    }
                    errorNotifyingServices.Add(tobiiEyeXPointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        tobiiEyeXPointService);
                    break;

                case PointsSources.VisualInteractionMyGaze:
                    var myGazePointService = new MyGazePointService();
                    errorNotifyingServices.Add(myGazePointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        myGazePointService);
                    break;

                default:
                    throw new ArgumentException("'PointsSource' settings is missing or not recognised! Please correct and restart OptiKey.");
            }

            //Instantiate key trigger source
            ITriggerSource keySelectionTriggerSource;
            switch (Settings.Default.KeySelectionTriggerSource)
            {
                case TriggerSources.Fixations:
                    keySelectionTriggerSource = new KeyFixationSource(
                       Settings.Default.KeySelectionTriggerFixationLockOnTime,
                       Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn,
                       Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime,
                       Settings.Default.KeySelectionTriggerFixationCompleteTimesByIndividualKey
                        ? Settings.Default.KeySelectionTriggerFixationCompleteTimesByKeyValues
                        : null,
                       Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                       pointSource);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    keySelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey,
                        pointSource);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    keySelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.KeySelectionTriggerMouseDownUpButton,
                        pointSource);
                    break;

                default:
                    throw new ArgumentException(
                        "'KeySelectionTriggerSource' setting is missing or not recognised! Please correct and restart OptiKey.");
            }

            //Instantiate point trigger source
            ITriggerSource pointSelectionTriggerSource;
            switch (Settings.Default.PointSelectionTriggerSource)
            {
                case TriggerSources.Fixations:
                    pointSelectionTriggerSource = new PointFixationSource(
                        Settings.Default.PointSelectionTriggerFixationLockOnTime,
                        Settings.Default.PointSelectionTriggerFixationCompleteTime,
                        Settings.Default.PointSelectionTriggerLockOnRadiusInPixels,
                        Settings.Default.PointSelectionTriggerFixationRadiusInPixels,
                        pointSource);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    pointSelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey,
                        pointSource);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    pointSelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.PointSelectionTriggerMouseDownUpButton,
                        pointSource);
                    break;

                default:
                    throw new ArgumentException(
                        "'PointSelectionTriggerSource' setting is missing or not recognised! "
                        + "Please correct and restart OptiKey.");
            }

            var inputService = new InputService(keyStateService, dictionaryService, audioService, capturingStateManager,
                pointSource, keySelectionTriggerSource, pointSelectionTriggerSource);
            inputService.RequestSuspend(); //Pause it initially
            return inputService;
        }

        #endregion

        #region Log Diagnostic Info

        private static void LogDiagnosticInfo()
        {
            Log.InfoFormat("Assembly version: {0}", DiagnosticInfo.AssemblyVersion);
            var assemblyFileVersion = DiagnosticInfo.AssemblyFileVersion;
            if (!string.IsNullOrEmpty(assemblyFileVersion))
            {
                Log.InfoFormat("Assembly file version: {0}", assemblyFileVersion);
            }
            if(DiagnosticInfo.IsApplicationNetworkDeployed)
            {
                Log.InfoFormat("ClickOnce deployment version: {0}", DiagnosticInfo.DeploymentVersion);
            }
            Log.InfoFormat("Running as admin: {0}", DiagnosticInfo.RunningAsAdministrator);
            Log.InfoFormat("Process elevated: {0}", DiagnosticInfo.IsProcessElevated);
            Log.InfoFormat("Process bitness: {0}", DiagnosticInfo.ProcessBitness);
            Log.InfoFormat("OS version: {0}", DiagnosticInfo.OperatingSystemVersion);
            Log.InfoFormat("OS service pack: {0}", DiagnosticInfo.OperatingSystemServicePack);
            Log.InfoFormat("OS bitness: {0}", DiagnosticInfo.OperatingSystemBitness);
        }

        #endregion

        #region Show Splash Screen

        private static async Task<bool> ShowSplashScreen(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.ShowSplashScreen)
            {
                Log.Info("Showing splash screen.");

                var message = new StringBuilder();

                message.AppendLine(string.Format(OptiKey.Properties.Resources.VERSION_DESCRIPTION, DiagnosticInfo.AssemblyVersion));
                message.AppendLine(string.Format(OptiKey.Properties.Resources.KEYBOARD_AND_DICTIONARY_LANGUAGE_DESCRIPTION, Settings.Default.KeyboardAndDictionaryLanguage.ToDescription()));
                message.AppendLine(string.Format(OptiKey.Properties.Resources.UI_LANGUAGE_DESCRIPTION, Settings.Default.UiLanguage.ToDescription()));
                message.AppendLine(string.Format(OptiKey.Properties.Resources.POINTING_SOURCE_DESCRIPTION, Settings.Default.PointsSource.ToDescription()));

                var keySelectionSb = new StringBuilder();
                keySelectionSb.Append(Settings.Default.KeySelectionTriggerSource.ToDescription());
                switch (Settings.Default.KeySelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        keySelectionSb.Append(string.Format(OptiKey.Properties.Resources.DURATION_FORMAT, Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime.TotalMilliseconds));
                        break;

                    case TriggerSources.KeyboardKeyDownsUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey));
                        break;

                    case TriggerSources.MouseButtonDownUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerMouseDownUpButton));
                        break;
                }

                message.AppendLine(string.Format(OptiKey.Properties.Resources.KEY_SELECTION_TRIGGER_DESCRIPTION, keySelectionSb));

                var pointSelectionSb = new StringBuilder();
                pointSelectionSb.Append(Settings.Default.PointSelectionTriggerSource.ToDescription());
                switch (Settings.Default.PointSelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        pointSelectionSb.Append(string.Format(OptiKey.Properties.Resources.DURATION_FORMAT, Settings.Default.PointSelectionTriggerFixationCompleteTime.TotalMilliseconds));
                        break;

                    case TriggerSources.KeyboardKeyDownsUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey));
                        break;

                    case TriggerSources.MouseButtonDownUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerMouseDownUpButton));
                        break;
                }

                message.AppendLine(string.Format(OptiKey.Properties.Resources.POINT_SELECTION_DESCRIPTION, pointSelectionSb));
                message.AppendLine(OptiKey.Properties.Resources.MANAGEMENT_CONSOLE_DESCRIPTION);
                message.AppendLine(OptiKey.Properties.Resources.WEBSITE_DESCRIPTION);

                inputService.RequestSuspend();
                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                mainViewModel.RaiseToastNotification(
                    OptiKey.Properties.Resources.OPTIKEY_DESCRIPTION,
                    message.ToString(),
                    NotificationTypes.Normal,
                    () =>
                        {
                            inputService.RequestResume();
                            taskCompletionSource.SetResult(true);
                        });
            }
            else
            {
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
        }

        #endregion

        #region  Check For Updates

        private static async Task<bool> CheckForUpdates(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            try
            {
                if (Settings.Default.CheckForUpdates)
                {
                    Log.InfoFormat("Checking GitHub for updates (repo owner:'{0}', repo name:'{1}').", GitHubRepoOwner, GitHubRepoName);

                    var github = new GitHubClient(new ProductHeaderValue("OptiKey"));
                    var releases = await github.Repository.Release.GetAll(GitHubRepoOwner, GitHubRepoName);
                    var latestRelease = releases.FirstOrDefault(release => !release.Prerelease);
                    if (latestRelease != null)
                    {
                        var currentVersion = new Version(DiagnosticInfo.AssemblyVersion); //Convert from string

                        //Discard revision (4th number) as my GitHub releases are tagged with "vMAJOR.MINOR.PATCH"
                        currentVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                        if (!string.IsNullOrEmpty(latestRelease.TagName))
                        {
                            var tagNameWithoutLetters =
                                new string(latestRelease.TagName.ToCharArray().Where(c => !char.IsLetter(c)).ToArray());
                            var latestAvailableVersion = new Version(tagNameWithoutLetters);
                            if (latestAvailableVersion > currentVersion)
                            {
                                Log.InfoFormat(
                                    "An update is available. Current version is {0}. Latest version on GitHub repo is {1}",
                                    currentVersion, latestAvailableVersion);

                                inputService.RequestSuspend();
                                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                                mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.UPDATE_AVAILABLE,
                                    string.Format(OptiKey.Properties.Resources.URL_DOWNLOAD_PROMPT, latestRelease.TagName),
                                    NotificationTypes.Normal,
                                     () =>
                                     {
                                         inputService.RequestResume();
                                         taskCompletionSource.SetResult(true);
                                     });
                            }
                            else
                            {
                                Log.Info("No update found.");
                                taskCompletionSource.SetResult(false);
                            }
                        }
                        else
                        {
                            Log.Info("Unable to determine if an update is available as the latest release lacks a tag.");
                            taskCompletionSource.SetResult(false);
                        }
                    }
                    else
                    {
                        Log.Info("No releases found.");
                        taskCompletionSource.SetResult(false);
                    }
                }
                else
                {
                    Log.Info("Check for update is disabled - skipping check.");
                    taskCompletionSource.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error when checking for updates. Exception message:{0}\nStackTrace:{1}", ex.Message, ex.StackTrace);
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
        }

        #endregion

        #region Release Keys On App Exit

        private static void ReleaseKeysOnApplicationExit(IKeyStateService keyStateService, IPublishService publishService)
        {
            Current.Exit += (o, args) =>
            {
                if (keyStateService.SimulateKeyStrokes)
                {
                    publishService.ReleaseAllDownKeys();
                }
            };
        }

        #endregion

        #region Validate Dynamic Keyboard Location

        private static string GetDefaultUserKeyboardFolder()
        {
            const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Keyboards\";

            var applicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ApplicationDataSubPath);
            Directory.CreateDirectory(applicationDataPath); //Does nothing if already exists                        
            return applicationDataPath;
        }

        private static void ValidateDynamicKeyboardLocation()
        {
            if (string.IsNullOrEmpty(Settings.Default.DynamicKeyboardsLocation))
            {
                // First time we set to APPDATA location, user may move through settings later
                Settings.Default.DynamicKeyboardsLocation = GetDefaultUserKeyboardFolder(); ;
            }
        }

        #endregion

        #region Validate Plugins Location

        private static string GetDefaultPluginsFolder()
        {
            const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Plugins\";

            var applicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ApplicationDataSubPath);

            // If directory doesn't exist, assume that this is the first run. So, move plugins from installation package to target path
            if (!Directory.Exists(applicationDataPath))
            {
                Directory.CreateDirectory(applicationDataPath);
                foreach (string pluginFile in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\Plugins"))
                {
                    File.Copy(pluginFile, Path.Combine(applicationDataPath, Path.GetFileName(pluginFile)), true);
                }
            }

            return applicationDataPath;
        }

        private static void ValidatePluginsLocation()
        {
            if (string.IsNullOrEmpty(Settings.Default.PluginsLocation))
            {
                // First time we set to APPDATA location, user may move through settings later
                Settings.Default.PluginsLocation = GetDefaultPluginsFolder(); ;
            }
        }

        #endregion

        #region Clean Up Extracted CommuniKate Files If Staged For Deletion

        private static void CleanupAndPrepareCommuniKateInitialState()
        {
            if (Settings.Default.EnableCommuniKateKeyboardLayout)
            {
                if (Settings.Default.CommuniKateStagedForDeletion)
                {
                    Log.Info("Deleting previously unpacked CommuniKate pageset.");
                    string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\CommuniKate\";
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    Log.Info("Previously unpacked CommuniKate pageset deleted successfully.");
                }
                Settings.Default.CommuniKateStagedForDeletion = false;
                
                Settings.Default.UsingCommuniKateKeyboardLayout = Settings.Default.UseCommuniKateKeyboardLayoutByDefault;
                Settings.Default.CommuniKateKeyboardCurrentContext = null;
                Settings.Default.CommuniKateKeyboardPrevious1Context = "_null_";
                Settings.Default.CommuniKateKeyboardPrevious2Context = "_null_";
                Settings.Default.CommuniKateKeyboardPrevious3Context = "_null_";
                Settings.Default.CommuniKateKeyboardPrevious4Context = "_null_";
            }
        }

        #endregion

        #region Attempt To Start/Stop Mary TTS Service Automatically

        private static async Task<bool> AttemptToStartMaryTTSService(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.MaryTTSEnabled)
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized, // cannot close it if set to hidden
                        CreateNoWindow = true
                    }
                };

                if (!File.Exists(Settings.Default.MaryTTSLocation))
                {
                    Log.InfoFormat("Failed to started MaryTTS (setting MaryTTSLocation does represent an existing file). " +
                        "Disabling MaryTTS and using System Voice '{0}' instead.", Settings.Default.SpeechVoice);
                    Settings.Default.MaryTTSEnabled = false;

                    inputService.RequestSuspend();
                    mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.MARYTTS_UNAVAILABLE,
                        string.Format(OptiKey.Properties.Resources.USING_DEFAULT_VOICE, Settings.Default.SpeechVoice),
                        NotificationTypes.Error, () =>
                        {
                            inputService.RequestResume();
                            taskCompletionSource.SetResult(false);
                        });
                }
                else if (Settings.Default.MaryTTSLocation.EndsWith(ExpectedMaryTTSLocationSuffix))
                {
                    proc.StartInfo.FileName = Settings.Default.MaryTTSLocation;

                    Log.InfoFormat("Trying to start MaryTTS from '{0}'.", proc.StartInfo.FileName);
                    try
                    {
                        proc.Start();
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = string.Format(
                            "Failed to started MaryTTS (exception encountered). Disabling MaryTTS and using System Voice '{0}' instead.", 
                            Settings.Default.SpeechVoice);
                        Log.Error(errorMsg, ex);
                        Settings.Default.MaryTTSEnabled = false;

                        inputService.RequestSuspend();
                        audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                        mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.MARYTTS_UNAVAILABLE,
                            string.Format(OptiKey.Properties.Resources.USING_DEFAULT_VOICE, Settings.Default.SpeechVoice),
                            NotificationTypes.Error, () =>
                            {
                                inputService.RequestResume();
                                taskCompletionSource.SetResult(false);
                            });
                    }

                    if (proc.StartTime <= DateTime.Now && !proc.HasExited)
                    {
                        Log.InfoFormat("Started MaryTTS at {0}.", proc.StartTime);
                        proc.CloseOnApplicationExit(Log, "MaryTTS");
                        taskCompletionSource.SetResult(true);
                    }
                    else
                    {
                        var errorMsg = string.Format(
                            "Failed to started MaryTTS (server not running). Disabling MaryTTS and using System Voice '{0}' instead.",
                            Settings.Default.SpeechVoice);

                        if (proc.HasExited)
                        {
                            errorMsg = string.Format(
                            "Failed to started MaryTTS (server was closed). Disabling MaryTTS and using System Voice '{0}' instead.",
                            Settings.Default.SpeechVoice);
                        }

                        Log.Error(errorMsg);
                        Settings.Default.MaryTTSEnabled = false;

                        inputService.RequestSuspend();
                        audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                        mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.MARYTTS_UNAVAILABLE,
                            string.Format(OptiKey.Properties.Resources.USING_DEFAULT_VOICE, Settings.Default.SpeechVoice),
                            NotificationTypes.Error, () =>
                            {
                                inputService.RequestResume();
                                taskCompletionSource.SetResult(false);
                            });
                    }
                }
                else
                {
                    Log.InfoFormat("Failed to started MaryTTS (setting MaryTTSLocation does not end in the expected suffix '{0}'). " +
                        "Disabling MaryTTS and using System Voice '{1}' instead.", ExpectedMaryTTSLocationSuffix, Settings.Default.SpeechVoice);
                    Settings.Default.MaryTTSEnabled = false;

                    inputService.RequestSuspend();
                    mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.MARYTTS_UNAVAILABLE,
                        string.Format(OptiKey.Properties.Resources.USING_DEFAULT_VOICE, Settings.Default.SpeechVoice),
                        NotificationTypes.Error, () =>
                        {
                            inputService.RequestResume();
                            taskCompletionSource.SetResult(false);
                        });
                }
            }
            else
            {
                taskCompletionSource.SetResult(true);
            }
            
            return await taskCompletionSource.Task;
        }

        #endregion

        #region Alert If Presage Bitness Or Bootstrap Or Version Failure

        private static async Task<bool> AlertIfPresageBitnessOrBootstrapOrVersionFailure(
            bool presageInstallationProblem, IInputService inputService, IAudioService audioService,  MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (presageInstallationProblem)
            {
                Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                Log.Error("Invalid Presage installation, or problem starting Presage. Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                inputService.RequestSuspend();
                audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.PRESAGE_UNAVAILABLE,
                    string.Format(OptiKey.Properties.Resources.USING_DEFAULT_SUGGESTION_METHOD,
                        Settings.Default.SuggestionMethod),
                    NotificationTypes.Error, () =>
                    {
                        inputService.RequestResume();
                        taskCompletionSource.SetResult(false);
                    });
            }
            else 
            {
                if (Settings.Default.SuggestionMethod == SuggestionMethods.Presage)
                {
                    Log.Info("Presage installation validated.");
                }
                taskCompletionSource.SetResult(true);
            }

            return await taskCompletionSource.Task;
        }

        #endregion
    }
}
