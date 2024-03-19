// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
using Octokit;
using presage;
using Application = System.Windows.Application;
using JuliusSweetland.OptiKey.Services.PluginEngine;
using WindowsRecipes.TaskbarSingleInstance;

namespace JuliusSweetland.OptiKey
{
    /// <summary>
    /// Interaction logic for OptiKeyApp.xaml
    /// </summary>
    public partial class OptiKeyApp : Application
    {
        #region Constants

        protected const string GazeTrackerUdpRegex = @"^STREAM_DATA\s(?<instanceTime>\d+)\s(?<x>-?\d+(\.[0-9]+)?)\s(?<y>-?\d+(\.[0-9]+)?)";
        protected const string GitHubRepoName = "optikey";
        protected const string GitHubRepoOwner = "optikey";
        protected const string ExpectedMaryTTSLocationSuffix = @"\bin\marytts-server.bat";

        #endregion

        #region protected Member Vars

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected Action applyTheme;

        private bool persistedState = true;
        private WindowStates tempWindowState;
        private DockEdges tempDockPosition;
        private DockSizes tempDockSize;
        private double tempFullDockThickness;
        private double tempCollapsedDockThickness;
        private Rect tempFloatingSizeAndPosition;

        #endregion

        #region Ctor

        public OptiKeyApp()
        {

        }

        // This will be assigned from within derived apps
        protected static SingleInstanceManager _manager;

        public static void RestartApp()
        {
            // Shut down logging so that new app instance can roll over log files okay
            LogManager.Flush(1000);
            LogManager.Shutdown();

            // Release single-instance mutex if we've got one
            if (_manager != null)
            {
                _manager.Dispose();
            }
            System.Windows.Forms.Application.Restart();
        }

        // Previously in core OptiKey ctr, now called by derived classes after setting up Settings class
        protected void Initialise()
        {
            //Setup unhandled exception handling 
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

            //Migrate the completion time setting to the new field
            if (Settings.Default.KeySelectionTriggerFixationDefaultCompleteTimes == null)
            {
                Settings.Default.KeySelectionTriggerFixationDefaultCompleteTimes
                    = Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime.TotalMilliseconds.ToString();
            }

            // Output the user settings for debugging
            logUserSettings();

            //Adjust log4net logging level if in debug mode
            ((Hierarchy)LogManager.GetRepository()).Root.Level = Settings.Default.Debug ? Level.Debug : Level.Info;
            ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);

            //Apply resource language (and listen for changes)
            Action<Languages> applyResourceLanguage = language =>
            {
                OptiKey.Properties.Resources.Culture = language.ToCultureInfo(); //Reloads resource file by culture code
                Settings.Default.UiLanguageFlowDirection = language.ToCultureInfo().TextInfo.IsRightToLeft
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            };
            Settings.Default.OnPropertyChanges(s => s.UiLanguage).Subscribe(applyResourceLanguage);
            applyResourceLanguage(Settings.Default.UiLanguage);

            //Logic to initially apply the theme and change the theme on setting changes
            applyTheme = () =>
            {
                var themeDictionary = new ThemeResourceDictionary
                {
                    // TODO: this is a bit weird, makes assumptions about architecture of calling code...
                    Source = new Uri("pack://application:,,,/OptiKey;component" + Settings.Default.Theme)
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

        protected static bool PresageInstallationProblemsDetected()
        {
            if (Settings.Default.SuggestionMethod == SuggestionMethods.Presage)
            {
                //1.Check the install path to detect if the wrong bitness of Presage is installed
                string presagePath = null;
                string presageStartMenuFolder = null;
                string processBitness = DiagnosticInfo.ProcessBitness;
                
                try
                {
                    presagePath = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Presage", "", string.Empty).ToString();
                    presageStartMenuFolder = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Presage", "Start Menu Folder", string.Empty).ToString();
                }
                catch (Exception ex)
                {
                    Log.Error("Cannot find Presage entries in the registry, is it installed?");
                    Log.ErrorFormat("Caught exception: {0}", ex);
                }

                Log.InfoFormat("Presage path: {0} | Presage start menu folder: {1} | process bitness: {2}", presagePath, presageStartMenuFolder, processBitness);

                List<string> presageOptions = new List<string>();
                if (processBitness.Contains("64"))
                {
                    presageOptions.Add("presage - 0.9.1 - 64bit");
                    presageOptions.Add("presage-0.9.2~beta20150909-64bit");
                }
                else
                {
                    presageOptions.Add("presage - 0.9.1 - 32bit");
                    presageOptions.Add("presage-0.9.2~beta20150909-32bit");
                }

                if (string.IsNullOrEmpty(presagePath)
                    || string.IsNullOrEmpty(presageStartMenuFolder))
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    string msg = "Invalid Presage installation detected (path(s) missing).\n";
                    msg += $"Must install '{ String.Join("' or '", presageOptions.ToArray()) }'. Changed SuggestionMethod to NGram.";
                    Log.Error(msg);
                    return true;
                }

                if (presageStartMenuFolder != "presage-0.9.2~beta20150909"
                    && presageStartMenuFolder != "presage-0.9.1")
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    string msg = "Invalid Presage installation detected (valid version not detected).\n";
                    msg += $"Must install '{ String.Join("' or '", presageOptions.ToArray()) }'. Changed SuggestionMethod to NGram.";
                    Log.Error(msg);
                    return true;
                }

                // On Windows 32 bit, we will only be able to install 32 bit Presage and 32 bit Optikey so don't need to check bitness.
                // Presage's install location will be \Program Files\ since there is only one Program Files folder available. 

                // On Windows 64 bit, 32 bit apps get installed into Program Files (x86) and 64 bit apps into Program Files.
                // We need to check that Optikey and Presage have the same bitness
                if (DiagnosticInfo.OperatingSystemBitness.Contains("64")) {
                    if ((processBitness == "64-Bit" && presagePath != @"C:\Program Files\presage")
                        || (processBitness == "32-Bit" && presagePath != @"C:\Program Files (x86)\presage"))
                    {
                        Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                        Log.Error("Invalid Presage installation detected (incorrect bitness? Install location is suspect). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                        return true;
                    }
                }

                if (!Directory.Exists(presagePath))
                {
                    Settings.Default.SuggestionMethod = SuggestionMethods.NGram;
                    Log.Error("Invalid Presage installation detected (install directory does not exist). Must install 'presage-0.9.1-32bit' or 'presage-0.9.2~beta20150909-32bit'. Changed SuggesionMethod to NGram.");
                    return true;
                }

                //2.Ensure the presage db exists at the setting location
                if (!File.Exists(Settings.Default.PresageDatabaseLocation))
                {
                    try
                    {
                        string ApplicationDataSubPath = @"OptiKey\OptiKey";
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
                        var database = Path.Combine(path, @"Presage\database.db");
                        if (!File.Exists(database))
                        {
                            try
                            {
                                using (ZipArchive archive = ZipFile.Open(@".\Resources\Presage\database.zip", ZipArchiveMode.Read, Encoding.UTF8))
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
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Presage failed to bootstrap. The database (database.db file) was not found, and the database.zip file could not be extracted!", ex);
                        return true;
                    }
                }

                //3.Attempt to construct a Presage object, which can fail for a few reasons, including BadImageFormatExceptions (64-bit version installed)
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

                //Setup the presage config
                presageTestInstance.set_config("Presage.Predictors.DefaultSmoothedNgramPredictor.DBFILENAME", Path.GetFullPath(Settings.Default.PresageDatabaseLocation));
                presageTestInstance.set_config("Presage.Selector.REPEAT_SUGGESTIONS", "yes");
                presageTestInstance.set_config("Presage.Selector.SUGGESTIONS", Settings.Default.PresageNumberOfSuggestions.ToString());
                presageTestInstance.save_config();
                Log.Info("Presage settings set successfully.");
            }

            return false;
        }


        private void logUserSettings()
        {

            // If debug switched on, dump entire XML into log so we can replicate issues.
            if (Settings.Default.Debug)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
                Log.Debug(config.FilePath);

                // We read into a single string so we can print without log4net preamble on each line.
                string configText = File.ReadAllText(config.FilePath);
                Log.DebugFormat("\r\n{0}", configText);
            }
            // Otherwise just key: value pairs into log
            else
            {
                Log.Info("Current user settings:");

                foreach (SettingsProperty property in Settings.Default.Properties)
                {
                    Log.InfoFormat("  {0}: {1}", property.Name, Settings.Default[property.Name]);
                }
            }
        }


        #region Create Main Window Manipulation Service

        protected WindowManipulationService CreateMainWindowManipulationService(MainWindow mainWindow)
        {
            return new WindowManipulationService(
                mainWindow,
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        Log.DebugFormat("Getting persistedState with value '{0}'", persistedState);
                    }
                    return persistedState;
                },
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
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowState from settings with value '{0}'", Settings.Default.MainWindowState);
                        else
                            Log.DebugFormat("Getting tempWindowState with value '{0}'", Settings.Default.MainWindowState);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowState;
                    else
                        return tempWindowState;
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
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowFloatingSizeAndPosition from settings with value '{0}'", Settings.Default.MainWindowFloatingSizeAndPosition);
                        else
                            Log.DebugFormat("Getting tempFloatingSizeAndPosition with value '{0}'", tempFloatingSizeAndPosition);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowFloatingSizeAndPosition;
                    else
                        return tempFloatingSizeAndPosition;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowDockPosition from settings with value '{0}'", Settings.Default.MainWindowDockPosition);
                        else
                            Log.DebugFormat("Getting tempDockPosition with value '{0}'", tempDockPosition);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowDockPosition;
                    else
                        return tempDockPosition;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowDockSize from settings with value '{0}'", Settings.Default.MainWindowDockSize);
                        else
                            Log.DebugFormat("Getting tempDockSize with value '{0}'", tempDockSize);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowDockSize;
                    else
                        return tempDockSize;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowFullDockThicknessAsPercentageOfScreen from settings with value '{0}'", Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen);
                        else
                            Log.DebugFormat("Getting tempFullDockThickness with value '{0}'", tempFullDockThickness);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen;
                    else
                        return tempFullDockThickness;
                },
                () =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Getting MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness from settings with value '{0}'", Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness);
                        else
                            Log.DebugFormat("Getting tempCollapsedDockThickness with value '{0}'", tempCollapsedDockThickness);
                    }
                    if (persistedState)
                        return Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness;
                    else
                        return tempCollapsedDockThickness;
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
                        Log.DebugFormat("Storing persistedState with value '{0}'", o);
                    }
                    persistedState = o;
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
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowState to settings with value '{0}'", state);
                        else
                            Log.DebugFormat("Storing tempWindowState with value '{0}'", state);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowState = state;
                    else
                        tempWindowState = state;
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
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowFloatingSizeAndPosition to settings with value '{0}'", rect);
                        else
                            Log.DebugFormat("Storing tempFloatingSizeAndPosition with value '{0}'", rect);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowFloatingSizeAndPosition = rect;
                    else
                        tempFloatingSizeAndPosition = rect;
                },
                pos =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowDockPosition to settings with value '{0}'", pos);
                        else
                            Log.DebugFormat("Storing tempDockPosition with value '{0}'", pos);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowDockPosition = pos;
                    else
                        tempDockPosition = pos;
                },
                size =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowDockSize to settings with value '{0}'", size);
                        else
                            Log.DebugFormat("Storing tempDockSize with value '{0}'", size);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowDockSize = size;
                    else
                        tempDockSize = size;
                },
                t =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowFullDockThicknessAsPercentageOfScreen to settings with value '{0}'", t);
                        else
                            Log.DebugFormat("Storing tempFullDockThickness with value '{0}'", t);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = t;
                    else
                        tempFullDockThickness = t;
                },
                t =>
                {
                    if (Settings.Default.Debug)
                    {
                        if (persistedState)
                            Log.DebugFormat("Storing MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness to settings with value '{0}'", t);
                        else
                            Log.DebugFormat("Storing tempCollapsedDockThickness with value '{0}'", t);
                    }
                    if (persistedState)
                        Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness = t;
                    else
                        tempCollapsedDockThickness = t;
                });
        }

        #endregion

        #region Attach Unhandled Exception Handlers

        // Make sure exceptions get logged, and a crash message appears
        protected static void AttachUnhandledExceptionHandlers()
        {
            Action CloseLogsAndShowCrashWindow = () =>
            {
#if !DEBUG
                LogManager.Flush(1000);
                LogManager.Shutdown();

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    CrashWindow crashWindow = CrashWindow.Instance;
                    if (!crashWindow.IsVisible)
                        crashWindow.ShowDialog();                    
                });
#endif
            };

            Current.DispatcherUnhandledException += (sender, args) =>
            {
                Log.Error("A DispatcherUnhandledException has been encountered...", args.Exception);
                CloseLogsAndShowCrashWindow();
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Error("An UnhandledException has been encountered...", args.ExceptionObject as Exception);
                CloseLogsAndShowCrashWindow();                
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error("An UnobservedTaskException has been encountered...", args.Exception);
                CloseLogsAndShowCrashWindow();
            };        
        }

        #endregion

        #region Handle Corrupt Settings

        protected static void HandleCorruptSettings()
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
                        RestartApp();
                    }
                    catch {} //Swallow any exceptions (e.g. DispatcherExceptions) - we're shutting down so it doesn't matter.
                }
                Current.Shutdown(); //Avoid the inevitable crash by shutting down gracefully
            }
        }

        #endregion

        #region Create Service Methods

        protected static ICalibrationService CreateCalibrationService()
        {
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.TheEyeTribe:
                    return new TheEyeTribeCalibrationService();

                case PointsSources.IrisbondDuo:
                    return new IrisbondDuoCalibrationService();

                case PointsSources.IrisbondHiru:
                    return new IrisbondHiruCalibrationService();

                case PointsSources.TobiiPcEyeGo:
                case PointsSources.TobiiPcEyeGoPlus:
                case PointsSources.TobiiPcEye5:
                case PointsSources.TobiiPcEyeMini:
                case PointsSources.TobiiX2_30:
                case PointsSources.TobiiX2_60:
                    // TODO: consider calling out to external calibration EXE
                    //return new TobiiEyeXCalibrationService();
                    break;
            }

            return null;
        }

        protected static IInputService CreateInputService(
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

                case PointsSources.IrisbondDuo:
                    var irisBondDuoPointService = new IrisbondDuoPointService();
                    errorNotifyingServices.Add(irisBondDuoPointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        irisBondDuoPointService);
                    break;

                case PointsSources.IrisbondHiru:
                    var irisBondHiruPointService = new IrisbondHiruPointService();
                    errorNotifyingServices.Add(irisBondHiruPointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        irisBondHiruPointService);
                    break;                

                case PointsSources.TouchScreenPosition:
                    pointSource = new TouchScreenPositionSource(
                        Settings.Default.PointTtl);                    
                    break;

                case PointsSources.TheEyeTribe:
                    var theEyeTribePointService = new TheEyeTribePointService();
                    errorNotifyingServices.Add(theEyeTribePointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        theEyeTribePointService);
                    break;

                
                case PointsSources.TobiiPcEyeGo:
                case PointsSources.TobiiPcEyeGoPlus:
                case PointsSources.TobiiPcEye5:
                case PointsSources.TobiiPcEyeMini:
                case PointsSources.TobiiX2_30:
                case PointsSources.TobiiX2_60:
                    var tobiiPointService = new TobiiPointService();
                 
                    errorNotifyingServices.Add(tobiiPointService);
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        tobiiPointService);
                    break;

                case PointsSources.MousePosition:
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);
                    break;

                default:                    
                    // We may get here if we've got a deprecated tracker enum - we will check that separately                                        
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);                    

                    break;
            }

            ITriggerSource eyeGestureTriggerSource = new EyeGestureSource(pointSource);

            //Instantiate key trigger source
            ITriggerSource keySelectionTriggerSource;

            //Instantiate point trigger source
            ITriggerSource pointSelectionTriggerSource;

            // FIXME: this logic is a hack
            if (Settings.Default.PointsSource == PointsSources.TouchScreenPosition)
            {
                keySelectionTriggerSource = (TouchScreenPositionSource)pointSource;
                pointSelectionTriggerSource = (TouchScreenPositionSource)pointSource;
            }
            else
            {

                switch (Settings.Default.KeySelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        keySelectionTriggerSource = new KeyFixationSource(
                           Settings.Default.KeySelectionTriggerFixationLockOnTime,
                           Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn,
                           Settings.Default.KeySelectionTriggerFixationDefaultCompleteTimes,
                           Settings.Default.KeySelectionTriggerFixationCompleteTimesByIndividualKey
                            ? Settings.Default.KeySelectionTriggerFixationCompleteTimesByKeyValues
                            : null,
                           Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                           Settings.Default.KeySelectionTriggerFixationResetMousePositionAfterKeyPressed,
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

                    case TriggerSources.XInputButtonDownUps:
                        keySelectionTriggerSource = new XInputButtonDownUpSource(
                            Settings.Default.KeySelectionTriggerGamepadXInputController,
                            Settings.Default.KeySelectionTriggerGamepadXInputButtonDownUpButton,
                            Settings.Default.GamepadTriggerHoldToRepeat,
                            Settings.Default.GamepadTriggerFirstRepeatMilliseconds,
                            Settings.Default.GamepadTriggerNextRepeatMilliseconds,
                            pointSource);
                        break;

                    case TriggerSources.DirectInputButtonDownUps:
                        keySelectionTriggerSource = new DirectInputButtonDownUpSource(
                            Settings.Default.KeySelectionTriggerGamepadDirectInputController,
                            Settings.Default.KeySelectionTriggerGamepadDirectInputButtonDownUpButton,
                            Settings.Default.GamepadTriggerHoldToRepeat,
                            Settings.Default.GamepadTriggerFirstRepeatMilliseconds,
                            Settings.Default.GamepadTriggerNextRepeatMilliseconds,
                            pointSource);
                        break;

                    default:
                        throw new ArgumentException(
                            "'KeySelectionTriggerSource' setting is missing or not recognised! Please correct and restart OptiKey.");
                }


                switch (Settings.Default.PointSelectionTriggerSource)
                {
                    case TriggerSources.Fixations:
                        pointSelectionTriggerSource = new PointFixationSource(
                            Settings.Default.PointSelectionTriggerFixationLockOnTime,
                            Settings.Default.PointSelectionTriggerFixationCompleteTime,
                            Settings.Default.PointSelectionTriggerLockOnRadiusInPixels,
                            Settings.Default.PointSelectionTriggerLockOnRadiusInPixels,
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

                    case TriggerSources.XInputButtonDownUps:
                        pointSelectionTriggerSource = new XInputButtonDownUpSource(
                            Settings.Default.PointSelectionTriggerGamepadXInputController,
                            Settings.Default.PointSelectionTriggerGamepadXInputButtonDownUpButton,
                            Settings.Default.GamepadTriggerHoldToRepeat,
                            Settings.Default.GamepadTriggerFirstRepeatMilliseconds,
                            Settings.Default.GamepadTriggerNextRepeatMilliseconds,
                            pointSource);
                        break;

                    case TriggerSources.DirectInputButtonDownUps:
                        pointSelectionTriggerSource = new DirectInputButtonDownUpSource(
                            Settings.Default.PointSelectionTriggerGamepadDirectInputController,
                            Settings.Default.PointSelectionTriggerGamepadDirectInputButtonDownUpButton,
                            Settings.Default.GamepadTriggerHoldToRepeat,
                            Settings.Default.GamepadTriggerFirstRepeatMilliseconds,
                            Settings.Default.GamepadTriggerNextRepeatMilliseconds,
                            pointSource);
                        break;

                    default:
                        throw new ArgumentException(
                            "'PointSelectionTriggerSource' setting is missing or not recognised! "
                            + "Please correct and restart OptiKey.");
                }
            }
            var inputService = new InputService(keyStateService, dictionaryService, audioService, capturingStateManager,
            pointSource, eyeGestureTriggerSource, keySelectionTriggerSource, pointSelectionTriggerSource);
            inputService.RequestSuspend(); //Pause it initially
            return inputService;
        }

        #endregion

        #region Log Diagnostic Info

        protected static void LogDiagnosticInfo()
        {
            Log.InfoFormat("Assembly version: {0}", DiagnosticInfo.AssemblyVersion);
            var assemblyFileVersion = DiagnosticInfo.AssemblyFileVersion;
            if (!string.IsNullOrEmpty(assemblyFileVersion))
            {
                Log.InfoFormat("Assembly file version: {0}", assemblyFileVersion);
            }
            if (DiagnosticInfo.IsApplicationNetworkDeployed)
                if (DiagnosticInfo.IsApplicationNetworkDeployed)
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

        protected static async Task<bool> ShowSplashScreen(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel, string splashTitle)
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

                    case TriggerSources.XInputButtonDownUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerGamepadXInputButtonDownUpButton));
                        break;

                    case TriggerSources.DirectInputButtonDownUps:
                        keySelectionSb.Append(string.Format(" ({0})", Settings.Default.KeySelectionTriggerGamepadDirectInputButtonDownUpButton));
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

                    case TriggerSources.XInputButtonDownUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerGamepadXInputButtonDownUpButton));
                        break;

                    case TriggerSources.DirectInputButtonDownUps:
                        pointSelectionSb.Append(string.Format(" ({0})", Settings.Default.PointSelectionTriggerGamepadDirectInputButtonDownUpButton));
                        break;
                }

                message.AppendLine(string.Format(OptiKey.Properties.Resources.POINT_SELECTION_DESCRIPTION, pointSelectionSb));
                message.AppendLine(OptiKey.Properties.Resources.MANAGEMENT_CONSOLE_DESCRIPTION);
                message.AppendLine(OptiKey.Properties.Resources.WEBSITE_DESCRIPTION);
                message.AppendLine("By Julius Sweetland : @OptiKey_Julius");

                inputService.RequestSuspend();
                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                mainViewModel.RaiseToastNotification(
                    splashTitle,
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

        protected static async Task<bool> CheckForUpdates(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
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
                                if (currentVersion.Major < 4 && latestAvailableVersion.Major >= 4)
                                {
                                    //There should be no update prompt to upgrade from v3 (or earlier) to v4 (or later) due to breaking changes
                                    Log.InfoFormat("An update is available, but with major breaking changes. The user will not be notified. " +
                                                   "Current version is {0}. Latest version on GitHub repo is {1}", currentVersion, latestAvailableVersion);
                                }
                                else
                                {
                                    Log.InfoFormat("An update is available. Current version is {0}. Latest version on GitHub repo is {1}",
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

        protected static void ReleaseKeysOnApplicationExit(IKeyStateService keyStateService, IPublishService publishService)
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

        #region Copying of installed resources 

        protected static string CopyResourcesFirstTime(string subDirectoryName)
        {
            // Ensure resources have been copied from Program Files to user's AppData folder

            var sourcePath = AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" + subDirectoryName;

            var destPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                           @"OptiKey\OptiKey\" + subDirectoryName);

            // If directory doesn't exist, assume that this is the first run. 
            // So, move resource from installation package to target path
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    File.Copy(file, 
                              Path.Combine(destPath, Path.GetFileName(file)), 
                              true);
                }
            }
            
            return destPath;
        }        

        protected static void ValidateEyeGestures()
        {
            // Check that eye gestures file is readable, reset to default otherwise
            var eyeGesturesFilePath = Settings.Default.EyeGestureFile;

            try
            {
                XmlEyeGestures.ReadFromFile(eyeGesturesFilePath);
            }
            catch
            {
                Settings.Default.EyeGesturesEnabled = false; // to be enabled from Management Console by user

                // Copy bundled gesture file/s
                var applicationDataPath = CopyResourcesFirstTime("EyeGestures");

                // Read into string also 
                eyeGesturesFilePath = Directory.GetFiles(applicationDataPath).First();
                try
                {
                    Settings.Default.EyeGestureString = XmlEyeGestures.ReadFromFile(eyeGesturesFilePath).WriteToString();
                }
                catch
                {
                    Log.ErrorFormat("Could not read from gestures file {0}", eyeGesturesFilePath);
#if DEBUG 
                    throw;  // This file gets installed by Optikey so if there's an exception here we want to know about it
#endif                    
                }
            }

            Settings.Default.EyeGestureFile = eyeGesturesFilePath;
        }

        protected static void ValidateDynamicKeyboardLocation()
        {
            if (string.IsNullOrEmpty(Settings.Default.DynamicKeyboardsLocation))
            {
                // First time we set to APPDATA location, user may move through settings later
                Settings.Default.DynamicKeyboardsLocation = CopyResourcesFirstTime("Keyboards");
            }
        } 

        protected static void ValidateEyeTrackerResources()
        {
            CopyResourcesFirstTime("EyeTrackerSupport");
        }

        protected static void ValidatePluginsLocation()
        {
            if (string.IsNullOrEmpty(Settings.Default.PluginsLocation))
            {
                // First time we set to APPDATA location, user may move through settings later
                Settings.Default.PluginsLocation = CopyResourcesFirstTime("Plugins");
            }
        }

        #endregion

        #region Clean Up Extracted CommuniKate Files If Staged For Deletion

        protected static void CleanupAndPrepareCommuniKateInitialState()
        {
            if (Settings.Default.EnableCommuniKateKeyboardLayout)
            {
                if (Settings.Default.CommuniKateStagedForDeletion)
                {
                    Log.Info("Deleting previously unpacked CommuniKate pageset.");
                    string ApplicationDataSubPath = @"OptiKey\OptiKey\CommuniKate\";
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

        protected static async Task<bool> AttemptToStartMaryTTSService(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
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
        
        #region Alert If Eye Tracker Deprecated

        protected static async Task<bool> AlertIfEyeTrackerDeprecated(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.PointsSource.IsObsolete())
            {
                inputService.RequestSuspend();
                audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.EYETRACKER_DEPRECATED,
                    string.Format(OptiKey.Properties.Resources.EYETRACKER_DEPRECATED_DETAILS,
                        Settings.Default.SuggestionMethod),
                    NotificationTypes.Error, () =>
                    {
                        inputService.RequestResume();
                        taskCompletionSource.SetResult(false);
                    });
            }
            else
            {
                taskCompletionSource.SetResult(true);
            }
            return await taskCompletionSource.Task;
        }
        #endregion

        #region Alert If Presage Bitness Or Bootstrap Or Version Failure

        protected static async Task<bool> AlertIfPresageBitnessOrBootstrapOrVersionFailure(
            bool presageInstallationProblem, IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
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
