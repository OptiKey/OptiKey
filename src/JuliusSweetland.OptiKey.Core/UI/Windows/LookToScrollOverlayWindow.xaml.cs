// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.Native.Common.Enums;
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var windowHandle = new WindowInteropHelper(this).Handle;
            Static.Windows.SetExtendedWindowStyle(windowHandle,
                Static.Windows.GetExtendedWindowStyle(windowHandle) | ExtendedWindowStyles.WS_EX_TRANSPARENT | ExtendedWindowStyles.WS_EX_TOOLWINDOW);
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
