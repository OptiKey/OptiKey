using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
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
using Octokit;
using Octokit.Reactive;
using Application = System.Windows.Application;
using InteractionRequest = Microsoft.Practices.Prism.Interactivity.InteractionRequest;

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
            //Attach unhandled exception handlers
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            //Log startup diagnostic info
            Log.Info("STARTING UP.");
            LogDiagnosticInfo();

            //Attach shutdown handler
            Application.Current.Exit += (o, args) => Log.Info("SHUTTING DOWN.");

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

            //Correct incorrect settings (e.g. unexpected combinations)
            if (Settings.Default.KeyboardSet == KeyboardsSets.SpeechOnly)
            {
                if (Settings.Default.SimulateKeyStrokes) Settings.Default.SimulateKeyStrokes = false;
                if (Settings.Default.MultiKeySelectionEnabled) Settings.Default.MultiKeySelectionEnabled = false;
            }
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
                
                //Create services
                var notifyErrorServices = new List<INotifyErrors>();
                IAudioService audioService = new AudioService();
                IDictionaryService dictionaryService = new DictionaryService();
                IPublishService publishService = new PublishService();
                ISuggestionService suggestionService = new SuggestionService();
                ICalibrationService calibrationService = CreateCalibrationService();
                ICapturingStateManager capturingStateManager = new CapturingStateManager(audioService);
                IKeyboardService keyboardService = new KeyboardService(suggestionService, capturingStateManager, calibrationService);
                IInputService inputService = CreateInputService(keyboardService, dictionaryService, audioService, capturingStateManager, notifyErrorServices);
                IOutputService outputService = new OutputService(keyboardService, suggestionService, publishService, dictionaryService);
                notifyErrorServices.Add(audioService);
                notifyErrorServices.Add(dictionaryService);
                notifyErrorServices.Add(publishService);
                notifyErrorServices.Add(inputService);

                //Release keys on application exit
                ReleaseKeysOnApplicationExit(keyboardService, publishService);

                //Compose UI
                var mainWindow = new MainWindow(audioService, dictionaryService, inputService);

                IWindowManipulationService moveAndResizeService = new WindowManipulationService(mainWindow);
                notifyErrorServices.Add(moveAndResizeService);

                var mainViewModel = new MainViewModel(
                    audioService, calibrationService, dictionaryService, publishService, 
                    keyboardService, suggestionService, capturingStateManager,
                    inputService, outputService, moveAndResizeService, notifyErrorServices);

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

                UpdateCheck(latestReleaseVersion =>
                {
                    inputService.State = RunningStates.Paused;
                    audioService.PlaySound(Settings.Default.InfoSoundFile);
                    mainViewModel.NotificationRequest.Raise(new InteractionRequest.Notification
                    {
                        Title = "UPDATE AVAILABLE!",
                        Content = string.Format("Please visit www.optikey.org to download latest version ({0}).\n\nYou can turn off update checks from the management console.", latestReleaseVersion)
                    }, __ => { inputService.State = RunningStates.Running; });
                });
            }
            catch (Exception ex)
            {
                Log.Error("Error starting up application", ex);
                throw;
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
            }

            return null;
        }

        private IInputService CreateInputService(
            IKeyboardService keyboardService,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            ICapturingStateManager capturingStateManager,
            List<INotifyErrors> notifyErrorServices)
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
                    notifyErrorServices.Add(theEyeTribePointService);
                    pointSource = new TheEyeTribeSource(
                        Settings.Default.PointTtl,
                        theEyeTribePointService);
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
                       Settings.Default.KeySelectionTriggerFixationCompleteTime,
                       Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                       pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    keySelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.SelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    keySelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.SelectionTriggerMouseDownUpButton,
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
                        Settings.Default.PointSelectionTriggerFixationRadius,
                        pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    pointSelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.SelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    pointSelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.SelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
                    break;

                default:
                    throw new ArgumentException(
                        "'PointSelectionTriggerSource' setting is missing or not recognised! "
                        + "Please correct and restart OptiKey.");
            }

            return new InputService(keyboardService, dictionaryService, audioService, capturingStateManager,
                pointSource, keySelectionTriggerSource, pointSelectionTriggerSource);
        }

        #endregion
        
        #region Log Diagnostic Info
        
        private void LogDiagnosticInfo()
        {
            Log.Info(string.Format("Assembly version: {0}", DiagnosticInfo.AssemblyVersion));
            var assemblyFileVersion = DiagnosticInfo.AssemblyFileVersion;
            if (!string.IsNullOrEmpty(assemblyFileVersion))
            {
                Log.Info(string.Format("Assembly file version: {0}", assemblyFileVersion));
            }
            if(DiagnosticInfo.IsApplicationNetworkDeployed)
            {
                Log.Info(string.Format("ClickOnce deployment version: {0}", DiagnosticInfo.DeploymentVersion));
            }
            Log.Info(string.Format("Process elevated: {0}", DiagnosticInfo.IsProcessElevated));
            Log.Info(string.Format("Process bitness: {0}", DiagnosticInfo.ProcessBitness));
            Log.Info(string.Format("OS version: {0}", DiagnosticInfo.OperatingSystemVersion));
            Log.Info(string.Format("OS service pack: {0}", DiagnosticInfo.OperatingSystemServicePack));
            Log.Info(string.Format("OS bitness: {0}", DiagnosticInfo.OperatingSystemBitness));
        }
        
        #endregion

        #region  Update Check

        private void UpdateCheck(Action<string> executeIfUpdateAvailable)
        {
            new ObservableGitHubClient(new ProductHeaderValue("OptiKey")).Release
                .GetAll("juliussweetland", "optikey")
                .Take(1)
                .ObserveOnDispatcher()
                .Subscribe(release =>
                {
                    var versionNumbers = DiagnosticInfo.AssemblyVersion.Split('.');

                    var assemblyVersionString = versionNumbers.Count() >= 3
                        ? string.Format("v{0}.{1}.{2}", versionNumbers[0], versionNumbers[1], versionNumbers[2])
                        : DiagnosticInfo.AssemblyVersion;
                    
                    if(!string.IsNullOrEmpty(release.TagName)
                        && !string.Equals(release.TagName, assemblyVersionString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Info(string.Format("An update is available. Current version is {0}. Latest version on GitHub repo is {1}", 
                            assemblyVersionString, release.TagName));

                        executeIfUpdateAvailable(release.TagName);
                    }
                });
        }

        #endregion

        #region Release Keys On App Exit

        private void ReleaseKeysOnApplicationExit(IKeyboardService keyboardService, IPublishService publishService)
        {
            Application.Current.Exit += (o, args) =>
            {
                if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
                {
                    publishService.ReleaseAllDownKeys();
                }
            };
        }

        #endregion

        #region Unhandled Exception Handlers

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            HandleUnhandledException(dispatcherUnhandledExceptionEventArgs.Exception);
            Application.Current.Shutdown();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            HandleUnhandledException(unhandledExceptionEventArgs.ExceptionObject as Exception);
            Application.Current.Shutdown();
        }

        private void HandleUnhandledException(Exception exception)
        {
            Log.Error("An unhandled error has occurred and the application needs to close. Exception details...", exception);
        }

        #endregion
    }
}
