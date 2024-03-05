// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services.PluginEngine;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for PointingAndSelectingView.xaml
    /// </summary>
    public partial class PointingAndSelectingView : System.Windows.Controls.UserControl
    {
        public PointingAndSelectingView()
        {
            InitializeComponent();            
        }

        void OpenPluginsWindow(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as PointingAndSelectingViewModel;
            if (viewModel != null)
            {       
                // Reset plugin engine
                viewModel.eyeTrackerPluginEngine.MostRecentlyInstalledDll = null; // reset this state before re-use
                viewModel.eyeTrackerPluginEngine.Refresh();

                var eyeTrackerPluginsWindow = new EyeTrackerPluginsWindow();
                eyeTrackerPluginsWindow.DataContext = viewModel.eyeTrackerPluginEngine;
                eyeTrackerPluginsWindow.Owner = Window.GetWindow(this);
                eyeTrackerPluginsWindow.Closing += (_, __) =>
                {
                    DllLoader.ResetTempDomain(); // release any locks on files
                    viewModel.UpdatePlugins();
                };
                eyeTrackerPluginsWindow.ShowDialog();

            }
        }

    }
}
