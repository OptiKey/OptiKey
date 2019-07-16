// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using JuliusSweetland.OptiKey.Symbol.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Services.PluginEngine;
using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.UI.ViewModels;
using JuliusSweetland.OptiKey.UI.Windows;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using NBug.Core.UI;
using Octokit;
using presage;
using log4net.Appender; //Do not remove even if marked as unused by Resharper - it is used by the Release build configuration
using NBug.Core.UI; //Do not remove even if marked as unused by Resharper - it is used by the Release build configuration
using Application = System.Windows.Application;

namespace JuliusSweetland.OptiKey.Symbol
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : OptiKeyApp
    {

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
     
        #endregion

        #region Ctor

        public App()
        {
            // Setup derived settings class
            Settings.Initialise();

            // Core setup for all OptiKey apps
            Initialise();
            
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
                if (Settings.Default.EnablePlugins)
                {
                    PluginEngine.LoadAvailablePlugins();
                }

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

                mainWindow.SetMainViewModel(mainViewModel);

                //Setup actions to take once main view is loaded (i.e. the view is ready, so hook up the services which kicks everything off)
                Action postMainViewLoaded = () =>
                {
                    mainViewModel.AttachErrorNotifyingServiceHandlers();
                    mainViewModel.AttachInputServiceEventHandlers();
                };

                mainWindow.AddOnMainViewLoadedAction(postMainViewLoaded);

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
                    await ShowSplashScreen(inputService, audioService, mainViewModel, OptiKey.Properties.Resources.OPTIKEY_DESCRIPTION);
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
                if (File.Exists(Settings.Default.PresageDatabaseLocation))
                {
                    presageTestInstance.set_config("Presage.Predictors.DefaultSmoothedNgramPredictor.DBFILENAME", Path.GetFullPath(Settings.Default.PresageDatabaseLocation));
                }
                else
                {
                    try
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
                    catch (Exception ex)
                    {
                        Log.Error("Presage failed to bootstrap. The database (database.db file) was not found, and the database.zip file could not be extracted!", ex);
                        return true;
                    }
                }

                presageTestInstance.set_config("Presage.Selector.REPEAT_SUGGESTIONS", "yes");
                presageTestInstance.set_config("Presage.Selector.SUGGESTIONS", Settings.Default.PresageNumberOfSuggestions.ToString());
                presageTestInstance.save_config();
                Log.Info("Presage settings set successfully.");
            }

            return false;
        }

        #endregion

    }
}
