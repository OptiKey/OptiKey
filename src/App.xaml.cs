using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Observables.PointAndKeyValueSources;
using JuliusSweetland.ETTA.Observables.TriggerSignalSources;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.ViewModels;
using JuliusSweetland.ETTA.UI.Windows;
using log4net;

namespace JuliusSweetland.ETTA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region On Startup

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Log.Info("ETTA STARTING UP");

                //Attach unhandled exception handlers
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                //Upgrade settings (if required) - this ensures that user settings migrate between version changes
                if (Settings.Default.SettingsUpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.SettingsUpgradeRequired = false;
                    Settings.Default.Save();
                }

                //Instantiate point source
                IPointAndKeyValueSource pointSource;
                switch (Settings.Default.PointsSource)
                {
                    case PointsSources.GazeTracker:
                        pointSource = new GazeTrackerSource(
                            Settings.Default.PointTtl,
                            Settings.Default.GazeTrackerUdpPort,
                            new Regex(Settings.Default.GazeTrackerUdpRegex));
                        break;

                    case PointsSources.TheEyeTribe:
                        pointSource = new TheEyeTribeSource(
                            Settings.Default.PointTtl,
                            new TheEyeTribePointService());
                        break;

                    case PointsSources.MousePosition:
                        pointSource = new MousePositionSource(
                            Settings.Default.PointTtl);
                        break;

                    default:
                        throw new ArgumentException("'PointsSource' settings is missing or not recognised! Please correct and restart ETTA.");
                }

                //Instantiate key trigger source
                ITriggerSignalSource keySelectionTriggerSource;
                switch (Settings.Default.KeySelectionTriggerSource)
                {
                    case TriggerSources.AggregatedFixations:
                        keySelectionTriggerSource = new AggregateKeyFixationSource(
                            Settings.Default.KeySelectionTriggerFixationMinPoints,
                            Settings.Default.KeySelectionTriggerFixationTime,
                            Settings.Default.PointTtl,
                            pointSource.Sequence);
                        break;

                    case TriggerSources.Fixations:
                        keySelectionTriggerSource = new KeyFixationSource(
                            Settings.Default.KeySelectionTriggerFixationMinPoints,
                            Settings.Default.KeySelectionTriggerFixationTime,
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
                        throw new ArgumentException("'KeySelectionTriggerSource' setting is missing or not recognised! Please correct and restart ETTA.");
                }

                //Instantiate point trigger source
                ITriggerSignalSource pointSelectionTriggerSource;
                switch (Settings.Default.PointSelectionTriggerSource)
                {
                    case TriggerSources.AggregatedFixations:
                        throw new ArgumentException("'PointSelectionTriggerSource' setting is AggregatedFixations which is not supported! Please correct and restart ETTA.");

                    case TriggerSources.Fixations:
                        pointSelectionTriggerSource = new PointFixationSource(
                            Settings.Default.PointSelectionTriggerFixationMinPoints,
                            Settings.Default.PointSelectionTriggerFixationRadius,
                            Settings.Default.PointSelectionTriggerFixationTime,
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
                            + "Please correct and restart ETTA.");
                }

                //Instantiation dictionary and input services
                var dictionaryService = new DictionaryService();
                var inputService = new InputService(
                    dictionaryService, pointSource, keySelectionTriggerSource, pointSelectionTriggerSource);
                
                //Compose main window
                var mainWindow = new MainWindow();
                mainWindow.InputOutputView.DataContext = new InputOutputViewModel(inputService);
                mainWindow.Show();

                //pointSource.Sequence.Dump("PointSource");
                keySelectionTriggerSource.Sequence.Dump("KeySelectionTriggerSource");
            }
            catch (Exception ex)
            {
                Log.Error("Error starting up application", ex);
                throw;
            }
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
            if (exception != null)
            {
                Log.Error("An unhandled error has occurred and the application needs to close. Exception details...", exception);
            }
        }

        #endregion
    }
}
