// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels;
using MahApps.Metro.Controls;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for ManagementWindow.xaml
    /// </summary>
    public partial class ManagementWindow : MetroWindow
    {
        public ManagementWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IWindowManipulationService windowManipulationService)
        {
            InitializeComponent();

            //Instantiate ManagementViewModel and set as DataContext of ManagementView
            var managementViewModel = new ManagementViewModel(audioService, dictionaryService, windowManipulationService);
            this.ManagementView.DataContext = managementViewModel;
        }
    }
}
