using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public abstract class KeyboardView : UserControl
    {
        protected KeyboardView(bool supportsCollapsedDock = false, bool shiftAware = false)
        {
            //Setup binding for SupportsCollapsedDock property
            SetBinding(SupportsCollapsedDockProperty, new Binding
            {
                Path = new PropertyPath("DataContext.KeyboardSupportsCollapsedDock"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            });
            
            //TODO: Add binding for ShiftAware
            SetBinding(ShiftAwareProperty, new Binding
            {
                Path = new PropertyPath("DataContext.TextOutputService.KeyboardIsShiftAware"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            });

            //Push DP bound values to targets on each load (when the keyboard has potentially changed)
            Loaded += (sender, args) =>
            {
                SupportsCollapsedDock = supportsCollapsedDock;
                ShiftAware = shiftAware;
            };
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
    }
}
