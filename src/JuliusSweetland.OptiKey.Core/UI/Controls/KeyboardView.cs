// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public abstract class KeyboardView : UserControl
    {
		private readonly bool supportsCollapsedDock;
		private readonly bool shiftAware;

        protected KeyboardView(bool supportsCollapsedDock = false, bool shiftAware = false)
        {
			this.supportsCollapsedDock = supportsCollapsedDock;
			this.shiftAware = shiftAware;

            //Setup binding for SupportsCollapsedDock property
            SetBinding(SupportsCollapsedDockProperty, new Binding
            {
                Path = new PropertyPath("DataContext.KeyboardSupportsCollapsedDock"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            });
            
            //Setup binding for ShiftAware property
            SetBinding(ShiftAwareProperty, new Binding
            {
                Path = new PropertyPath("DataContext.KeyboardOutputService.KeyboardIsShiftAware"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            });

			//Push DP bound values to targets on each load (when the keyboard has potentially changed)
			Loaded += OnLoaded;

            SetResourceReference(StyleProperty, "KeyboardViewStyle");
        }

        public static readonly DependencyProperty SupportsCollapsedDockProperty =
            DependencyProperty.Register("SupportsCollapsedDock", typeof(bool), typeof(KeyboardView), new PropertyMetadata(false));

        protected bool SupportsCollapsedDock
        {
            get { return (bool)GetValue(SupportsCollapsedDockProperty); }
            set { SetValue(SupportsCollapsedDockProperty, value); }
        }

        public static readonly DependencyProperty ShiftAwareProperty =
            DependencyProperty.Register("ShiftAware", typeof (bool), typeof (KeyboardView), new PropertyMetadata(false));

        public bool ShiftAware
        {
            get { return (bool) GetValue(ShiftAwareProperty); }
            set { SetValue(ShiftAwareProperty, value); }
        }

		protected virtual void OnLoaded(object sender, RoutedEventArgs e)
		{
			SupportsCollapsedDock = supportsCollapsedDock;
			ShiftAware = shiftAware;
		}
	}
}
