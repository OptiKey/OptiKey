using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NBug.Core.UI;
using Octokit;
using Octokit.Reactive;
using Application = System.Windows.Application;
using FileMode = System.IO.FileMode;

namespace JuliusSweetland.OptiKey
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            Application.Current.Exit += (o, args) => Log.Info("SHUTTING DOWN.");

            HandleCorruptSettings();

            //Upgrade settings (if required) - this ensures that user settings migrate between version changes
            if (Settings.Default.SettingsUpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgradeRequired = false;
                Settings.Default.Save();
            }

            //Adjust log4net logging level if in debug mode
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Settings.Default.Debug ? Level.Debug : Level.Info;
            ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);

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

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Log.Info("Boot strapping the services and UI.");

                //Apply theme
                applyTheme();
                
                //Create services
                var errorNotifyingServices = new List<INotifyErrors>();
                IAudioService audioService = new AudioService();
                IDictionaryService dictionaryService = new DictionaryService();
                IPublishService publishService = new PublishService();
                ISuggestionStateService suggestionService = new SuggestionStateService();
                ICalibrationService calibrationService = CreateCalibrationService();
                ICapturingStateManager capturingStateManager = new CapturingStateManager(audioService);
                ILastMouseActionStateManager lastMouseActionStateManager = new LastMouseActionStateManager();
                IKeyStateService keyStateService = new KeyStateService(suggestionService, capturingStateManager, lastMouseActionStateManager, calibrationService);
                IInputService inputService = CreateInputService(keyStateService, dictionaryService, audioService, calibrationService, capturingStateManager, errorNotifyingServices);
                IKeyboardOutputService keyboardOutputService = new KeyboardOutputService(keyStateService, suggestionService, publishService, dictionaryService);
                IMouseOutputService mouseOutputService = new MouseOutputService(publishService);
                errorNotifyingServices.Add(audioService);
                errorNotifyingServices.Add(dictionaryService);
                errorNotifyingServices.Add(publishService);
                errorNotifyingServices.Add(inputService);

                //Release keys on application exit
                ReleaseKeysOnApplicationExit(keyStateService, publishService);

                //Compose UI
                var mainWindow = new MainWindow(audioService, dictionaryService, inputService);

                IWindowManipulationService mainWindowManipulationService = new WindowManipulationService(
                    mainWindow,
                    () => Settings.Default.MainWindowOpacity,
                    () => Settings.Default.MainWindowState,
                    () => Settings.Default.MainWindowPreviousState,
                    () => Settings.Default.MainWindowFloatingSizeAndPosition,
                    () => Settings.Default.MainWindowDockPosition,
                    () => Settings.Default.MainWindowDockSize,
                    () => Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen,
                    () => Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness,
                    o => { Settings.Default.MainWindowOpacity = o; Settings.Default.Save(); },
                    state => { Settings.Default.MainWindowState = state; Settings.Default.Save(); },
                    state => { Settings.Default.MainWindowPreviousState = state; Settings.Default.Save(); },
                    rect => { Settings.Default.MainWindowFloatingSizeAndPosition = rect; Settings.Default.Save(); },
                    pos => { Settings.Default.MainWindowDockPosition = pos; Settings.Default.Save(); },
                    size => { Settings.Default.MainWindowDockSize = size; Settings.Default.Save(); },
                    t => { Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = t; Settings.Default.Save(); },
                    t => { Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness = t; Settings.Default.Save(); });

                errorNotifyingServices.Add(mainWindowManipulationService);
                
                var mainViewModel = new MainViewModel(
                    audioService, calibrationService, dictionaryService, keyStateService,
                    suggestionService, capturingStateManager, lastMouseActionStateManager,
                    inputService, keyboardOutputService, mouseOutputService, mainWindowManipulationService, errorNotifyingServices);

                mainWindow.MainView.DataContext = mainViewModel;

                //Setup actions to take once main view is loaded (i.e. the view is ready, so hook up the services which kicks everything off)
                Action postMainViewLoaded = mainViewModel.AttachServiceEventHandlers;
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

                //Display splash screen and check for updates (and display message) after the window has been sized and positioned for the 1st time
                EventHandler sizeAndPositionInitialised = null;
                sizeAndPositionInitialised = async (_, __) =>
                {
                    mainWindowManipulationService.SizeAndPositionInitialised -= sizeAndPositionInitialised; //Ensure this handler only triggers once
                    await ShowSplashScreen(inputService, audioService, mainViewModel);
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

        #endregion

        #region Attach Unhandled Exception Handlers

        private static void AttachUnhandledExceptionHandlers()
        {
            Application.Current.DispatcherUnhandledException += (sender, args) => Log.Error("A DispatcherUnhandledException has been encountered...", args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error("An UnhandledException has been encountered...", args.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) => Log.Error("An UnobservedTaskException has been encountered...", args.Exception);

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
                    using (var fs = new FileStream(rootAppender.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
        }

        #endregion

        #region Handle Corrupt Settings

        private void HandleCorruptSettings()
        {
            try
            {
                //Attempting to read a setting from a corrupt user config file throws an exception
                var upgradeRequired = Settings.Default.SettingsUpgradeRequired;
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;

                if (MessageBox.Show(
                        "OptiKey has detected that your user settings file has become corrupted and must be repaired. " +
                        "This will be done by restoring an old version, or a default version if that isn't possible.\n\n" +
                        "Click Yes to reset your user settings and restart.\n\n" +
                        "Click No to close so that you can manually repair or copy your user settings file.",
                        "Uh-oh! The user settings file looks to be corrupt.",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    File.Delete(filename);
                    System.Windows.Forms.Application.Restart();
                }
                Application.Current.Shutdown(); //Avoid the inevitable crash by shutting down gracefully
            }
        }

        #endregion

        #region Create Service Methods

        private ICalibrationService CreateCalibrationService()
        {
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.TheEyeTribe:
                    return new TheEyeTribeCalibrationService();

                case PointsSources.TobiiEyeX:
                case PointsSources.TobiiRex:
                case PointsSources.TobiiPcEyeGo:
                    return new TobiiEyeXCalibrationService();
            }

            return null;
        }

        private IInputService CreateInputService(
            IKeyStateService keyStateService,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            ICalibrationService calibrationService,
            ICapturingStateManager capturingStateManager,
            List<INotifyErrors> errorNotifyingServices)
        {
            Log.Debug("Creating InputService.");

            //Instantiate point source
            IPointSource pointSource;
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.GazeTracker:
                    pointSource = new GazeTrackerSource(
                        Settings.Default.PointTtl,
                        Settings.Default.GazeTrackerUdpPort,
                        new Regex(Settings.Default.GazeTrackerUdpRegex));
                    break;

                case PointsSources.TheEyeTribe:
                    var theEyeTribePointService = new TheEyeTribePointService();
                    errorNotifyingServices.Add(theEyeTribePointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        theEyeTribePointService);
                    break;

                case PointsSources.TobiiEyeX:
                case PointsSources.TobiiRex:
                case PointsSources.TobiiPcEyeGo:
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

                case PointsSources.MousePosition:
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);
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
                       Settings.Default.KeySelectionTriggerFixationCompleteTime,
                       Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                       pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    keySelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    keySelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.KeySelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
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
                        pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    pointSelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    pointSelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.PointSelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
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
        
        private void LogDiagnosticInfo()
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
            Log.InfoFormat("Process elevated: {0}", DiagnosticInfo.IsProcessElevated);
            Log.InfoFormat("Process bitness: {0}", DiagnosticInfo.ProcessBitness);
            Log.InfoFormat("OS version: {0}", DiagnosticInfo.OperatingSystemVersion);
            Log.InfoFormat("OS service pack: {0}", DiagnosticInfo.OperatingSystemServicePack);
            Log.InfoFormat("OS bitness: {0}", DiagnosticInfo.OperatingSystemBitness);
        }
        
        #endregion

        #region Show Splash Screen

        private async Task<bool> ShowSplashScreen(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.ShowSplashScreen)
            {
                Log.Debug("Showing splash screen.");

                var message = new StringBuilder();

                message.AppendLine(string.Format("Version: {0}", DiagnosticInfo.AssemblyVersion));
                message.AppendLine(string.Format("Language: {0}", Settings.Default.Language.ToDescription()));
                message.AppendLine(string.Format("Pointing: {0}", Settings.Default.PointsSource.ToDescription()));

                var keySelectionSb = new StringBuilder();
                keySelectionSb.Append(Settings.Default.KeySelectionTriggerSource.ToDescription());
                switch (Settings.Default.KeySelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        keySelectionSb.Append(string.Format(" ({0:#,###}ms)", Settings.Default.KeySelectionTriggerFixationCompleteTime.TotalMilliseconds));
                        break;

                    case TriggerSources.KeyboardKeyDownsUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey));
                        break;

                    case TriggerSources.MouseButtonDownUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerMouseDownUpButton));
                        break;
                }
                message.AppendLine(string.Format("Key selection: {0}", keySelectionSb));

                var pointSelectionSb = new StringBuilder();
                pointSelectionSb.Append(Settings.Default.PointSelectionTriggerSource.ToDescription());
                switch (Settings.Default.PointSelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        pointSelectionSb.Append(string.Format(" ({0:#,###}ms)", Settings.Default.PointSelectionTriggerFixationCompleteTime.TotalMilliseconds));
                        break;

                    case TriggerSources.KeyboardKeyDownsUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey));
                        break;

                    case TriggerSources.MouseButtonDownUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerMouseDownUpButton));
                        break;
                }
                message.AppendLine(string.Format("Point selection: {0}", pointSelectionSb));

                message.AppendLine("Management console: ALT + M");
                message.AppendLine("Website: www.optikey.org");

                inputService.RequestSuspend();
                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                mainViewModel.RaiseToastNotification(
                    "OptiKey : Type · Click · Speak", 
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

        private async Task<bool> CheckForUpdates(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.CheckForUpdates)
            {
                Log.InfoFormat("Checking GitHub for updates (repo owner:'{0}', repo name:'{1}').", 
                    Settings.Default.GitHubRepoOwner, Settings.Default.GitHubRepoName);

                new ObservableGitHubClient(new ProductHeaderValue("OptiKey")).Release
                    .GetAll(Settings.Default.GitHubRepoOwner, Settings.Default.GitHubRepoName)
                    .Where(release => !release.Prerelease)
                    .Take(1)
                    .ObserveOnDispatcher()
                    .Subscribe(release =>
                    {
                        var currentVersion = new Version(DiagnosticInfo.AssemblyVersion); //Convert from string

                        //Discard revision (4th number) as my GitHub releases are tagged with "vMAJOR.MINOR.PATCH"
                        currentVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                        if (!string.IsNullOrEmpty(release.TagName))
                        {
                            var tagNameWithoutLetters =
                                new string(release.TagName.ToCharArray().Where(c => !char.IsLetter(c)).ToArray());
                            var latestAvailableVersion = new Version(tagNameWithoutLetters);
                            if (latestAvailableVersion > currentVersion)
                            {
                                Log.InfoFormat("An update is available. Current version is {0}. Latest version on GitHub repo is {1}",
                                    currentVersion, latestAvailableVersion);

                                inputService.RequestSuspend();
                                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                                mainViewModel.RaiseToastNotification("UPDATE AVAILABLE!",
                                    string.Format("Please visit www.optikey.org to download latest version ({0})\nYou can turn off update checks from the Management Console (ALT + M).", release.TagName),
                                    NotificationTypes.Normal,
                                    () => 
                                        {
                                            inputService.RequestResume();
                                            taskCompletionSource.SetResult(true);
                                        });

                                return;
                            }
                        }

                        Log.Info("No update found.");
                    }, exception => Log.InfoFormat("Error when checking for updates. Exception message:{0}", exception.Message));
            }
            else
            {
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
        }

        #endregion

        #region Release Keys On App Exit

        private void ReleaseKeysOnApplicationExit(IKeyStateService keyStateService, IPublishService publishService)
        {
            Application.Current.Exit += (o, args) =>
            {
                if (keyStateService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
                {
                    publishService.ReleaseAllDownKeys();
                }
            };
        }

        #endregion
    }
}
