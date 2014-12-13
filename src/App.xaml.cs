using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.UI.ViewModels;
using JuliusSweetland.ETTA.UI.Windows;
using log4net;
using log4net.Core;

namespace JuliusSweetland.ETTA
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
                Log.Info("ETTA STARTING UP");

                //Attach unhandled exception handlers
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                //Attach shutdown handler
                Application.Current.Exit += (o, args) => Log.Info("ETTA SHUTTING DOWN");

                //Adjust log4net logging level if in debug mode
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).Root.Level = Settings.Default.Debug ? Level.Debug : Level.Info;
                ((log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
                
                //Upgrade settings (if required) - this ensures that user settings migrate between version changes
                if (Settings.Default.SettingsUpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.SettingsUpgradeRequired = false;
                    Settings.Default.Save();
                }
                
                //Apply theme
                applyTheme();
                
                //Compose main window and apply theme
                var mainWindow = new MainWindow();
                var mainViewModel = new MainViewModel();
                mainWindow.MainView.DataContext = mainViewModel;
                
                if(mainWindow.MainView.IsLoaded)
                {
                    mainViewModel.Initialise();
                }
                else
                {
                    RoutedEventHandler loadedHandler = null;
                    loadedHandler = (s, a) =>
                    {
                        mainViewModel.Initialise();
                        mainWindow.MainView.Loaded -= loadedHandler; //Ensure this handler only triggers once
                    };
                    mainWindow.MainView.Loaded += loadedHandler;
                }
                
                mainWindow.Show();
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
                try
                {
                    Log.Error("An unhandled error has occurred and the application needs to close. Exception details...", exception);
                    MessageBox.Show("An unhandled error has occurred and the application needs to close. Please check the logs for details.");
                }
                catch {} //Swallow exception with logging or displaying messagebox to avoid looped errors
            }
        }

        #endregion
    }
}
