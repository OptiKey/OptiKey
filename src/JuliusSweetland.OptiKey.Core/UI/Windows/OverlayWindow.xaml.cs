// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Popup
    {
        private Point point = new Point(0, 0);
        private double indicatorSize;
     
        public OverlayWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.IsOpen = true;

            Action applySize = () =>
            {
                indicatorSize = Settings.Default.GazeIndicatorSize;
            };
            Settings.Default.OnPropertyChanges(s => s.GazeIndicatorSize).Subscribe(_ => applySize());
            applySize();

            Action applyStyle = () =>
            {
                viewModel.ShowCrosshair = Settings.Default.GazeIndicatorStyle == GazeIndicatorStyles.Crosshair
                    || Settings.Default.GazeIndicatorStyle == GazeIndicatorStyles.Scope;
                viewModel.ShowMonical = Settings.Default.GazeIndicatorStyle == GazeIndicatorStyles.Monical
                    || Settings.Default.GazeIndicatorStyle == GazeIndicatorStyles.Scope;
            };
            Settings.Default.OnPropertyChanges(s => s.GazeIndicatorStyle).Subscribe(_ => applyStyle());
            applyStyle();

            //Calculate position based on CurrentPositionPoint
            viewModel.OnPropertyChanges(vm => vm.CurrentPositionPoint).Subscribe(cpp => Point = cpp);

        }

        private Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    var dpiPoint = this.GetTransformFromDevice().Transform(point);
                    //Offsets are in device independent pixels (DIP), but the incoming Point is in pixels
                    Width = Height = indicatorSize;
                    HorizontalOffset = dpiPoint.X + indicatorSize / 2;
                    VerticalOffset = dpiPoint.Y - indicatorSize / 2;
                }
            }
        }
    }
}

