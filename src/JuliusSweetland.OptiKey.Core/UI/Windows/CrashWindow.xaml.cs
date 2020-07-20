// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
        public CrashWindow()
        {
            InitializeComponent();

            this.Loaded += (sender, args) =>
            {
                var dt = new DispatcherTimer { Interval = new TimeSpan(0, 0, Settings.Default.AutoCloseCrashMessageSeconds) };
                dt.Tick += (o, eventArgs) =>
                {
                    this.Close();

                    // The first crash might attempt a restart, but subsequent crashes shouldn't
                    if (Settings.Default.AttemptRestartUponCrash && Settings.Default.CleanShutdown)
                    {
                        Settings.Default.CleanShutdown = false;
                        Settings.Default.Save();
                        OptiKeyApp.RestartApp();
                    }
                };
                dt.Start();
            };
        }
    }
}
