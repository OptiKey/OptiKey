using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        private async void App_OnStartup(object sender, StartupEventArgs e)
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
                ISuggestionStateService suggestionService = new SuggestionStateService();
                ICalibrationService calibrationService = CreateCalibrationService();
                ICapturingStateManager capturingStateManager = new CapturingStateManager(audioService);
                ILastMouseActionStateManager lastMouseActionStateManager = new LastMouseActionStateManager();
                IWindowStateService mainWindowStateService = new WindowStateService();
                IKeyboardService keyboardService = new KeyboardService(suggestionService, capturingStateManager, lastMouseActionStateManager, calibrationService, mainWindowStateService);
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

                mainWindowStateService.Window = mainWindow;

                IWindowManipulationService mainWindowManipulationService = new WindowManipulationService(mainWindow,
                    () => Settings.Default.MainWindowTop, d => Settings.Default.MainWindowTop = d,
                    () => Settings.Default.MainWindowLeft, d => Settings.Default.MainWindowLeft = d,
                    () => Settings.Default.MainWindowHeight, d => Settings.Default.MainWindowHeight = d,
                    () => Settings.Default.MainWindowWidth, d => Settings.Default.MainWindowWidth = d,
                    () => Settings.Default.MainWindowState, s => Settings.Default.MainWindowState = s,
                    Settings.Default, true, true);
                
                notifyErrorServices.Add(mainWindowManipulationService);

                var mainViewModel = new MainViewModel(
                    audioService, calibrationService, dictionaryService, keyboardService, 
                    suggestionService, capturingStateManager, lastMouseActionStateManager,
                    inputService, outputService, mainWindowManipulationService, notifyErrorServices);

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

                await ShowSplashScreen(inputService, audioService, mainViewModel);
                inputService.State = RunningStates.Running; //Start the input service
                await CheckForUpdates(inputService, audioService, mainViewModel);
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
                        Settings.Default.PointSelectionTriggerLockOnRadius,
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
                pointSource, keySelectionTriggerSource, pointSelectionTriggerSource)
            {
                State = RunningStates.Paused //Instantiate the InputService and immediately set to paused
            };
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

        #region Show Splash Screen

        private async Task<bool> ShowSplashScreen(IInputService inputService, IAudioService audioService, MainViewModel mainViewModel)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>(); //Used to make this method awaitable on the InteractionRequest callback

            if (Settings.Default.ShowSplashScreen)
            {
                Log.Debug("Showing splash screen.");

                inputService.State = RunningStates.Paused;
                audioService.PlaySound(Settings.Default.InfoSoundFile);
                mainViewModel.RaiseToastNotification("Welcome to Optikey!", 
                    "Website: www.optikey.org\n" + "Settings: press ALT + M", NotificationTypes.Normal,
                    () =>
                        {
                            inputService.State = RunningStates.Running;
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
                Log.Info(string.Format("Checking GitHub for updates (repo owner:'{0}', repo name:'{1}').", 
                    Settings.Default.GitHubRepoOwner, Settings.Default.GitHubRepoName));

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
                                Log.Info(string.Format("An update is available. Current version is {0}. Latest version on GitHub repo is {1}",
                                    currentVersion, latestAvailableVersion));

                                inputService.State = RunningStates.Paused;
                                audioService.PlaySound(Settings.Default.InfoSoundFile);
                                mainViewModel.RaiseToastNotification("UPDATE AVAILABLE!",
                                    string.Format("Please visit www.optikey.org to download latest version ({0})\nYou can turn off update checks from the Management Console (ALT + M).", release.TagName),
                                    NotificationTypes.Normal,
                                    () => 
                                        {
                                            inputService.State = RunningStates.Running;
                                            taskCompletionSource.SetResult(true);
                                        });

                                return;
                            }
                        }

                        Log.Info("No update found.");
                    }, exception => Log.Info(string.Format("Error when checking for updates. Exception message:{0}", exception.Message)));
            }
            else
            {
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
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
    }
}
