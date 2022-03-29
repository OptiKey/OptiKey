// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using System.Windows.Threading;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        
        private static readonly Lazy<CrashWindow> instance =
            new Lazy<CrashWindow>(() => new CrashWindow
            {
                Topmost = true,
                ShowActivated = true
            });
        public static CrashWindow Instance { get { return instance.Value; } }

        private CrashWindow()
        {
            InitializeComponent();

            this.Loaded += (sender, args) =>
            {
                var dt = new DispatcherTimer { Interval = new TimeSpan(0, 0, Settings.Default.AutoCloseCrashMessageSeconds) };
                dt.Tick += (o, eventArgs) =>
                {
                    // Shutdown or restart after crash window timeout                    
                    bool attemptRestart = Settings.Default.AttemptRestartUponCrash && Settings.Default.CleanShutdown;
                    Settings.Default.CleanShutdown = false;
                    Settings.Default.Save();

                    // If this is a new crash, attempt a restart, but don't allow repeat crashes to get into restart loop
                    if (attemptRestart)
                        OptiKeyApp.RestartApp();
                    else
                        Environment.Exit(0);                        
                };
                dt.Start();
            };
        }
    }
}
