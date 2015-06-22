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
                TxtCrashMessage.Text = string.Format(
                    "Bad news - OptiKey has tripped over its feet and has to close down.\n\nApologies for the inconvenience.\n\n{0}", 
                    Settings.Default.AutoRestartAfterCrash ? "OptiKey will attempt to restart in a few seconds." : "OptiKey will exit in a few seconds.");

                var dt = new DispatcherTimer { Interval = new TimeSpan(0, 0, Settings.Default.AutoCloseCrashMessageSeconds) };
                dt.Tick += (o, eventArgs) =>
                {
                    this.Close();

                    if (Settings.Default.AutoRestartAfterCrash)
                    {
                        System.Windows.Forms.Application.Restart();
                    }
                };
                dt.Start();
            };
        }
    }
}
