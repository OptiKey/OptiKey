using System;
using System.Threading;
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
                var dt = new DispatcherTimer();
                dt.Interval = new TimeSpan(0, 0, 15);
                dt.Tick += (o, eventArgs) =>
                {
                    this.Close();
                    //System.Windows.Forms.Application.Restart();
                };
                dt.Start();
                //Settings.Default.AutoCloseCrashMessageSeconds * 1000
            };
        }
    }
}
