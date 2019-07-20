// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.UI.ViewModels;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for LookToScrollOverlayWindow.xaml
    /// </summary>
    public partial class LookToScrollOverlayWindow : Window
    {
        private readonly ILookToScrollOverlayViewModel viewModel;

        public LookToScrollOverlayWindow(ILookToScrollOverlayViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = viewModel;
        }

        // Based on: https://stackoverflow.com/a/3367137/9091159
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Apply the WS_EX_TRANSPARENT flag to the overlay window so that mouse events will
            // pass through to any window underneath.
            var hWnd = new WindowInteropHelper(this).Handle;
            Static.Windows.SetWindowExTransparent(hWnd);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals("ActiveLookToScrollBounds", e.PropertyName))
            {
                Rect bounds = viewModel.ActiveLookToScrollBounds;
                if (!bounds.IsEmpty)
                {
                    Top = bounds.Top;
                    Left = bounds.Left;
                    Width = bounds.Width;
                    Height = bounds.Height;
                }
            }
        }
    }
}
