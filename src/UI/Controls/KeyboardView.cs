using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public abstract class KeyboardView : UserControl
    {
        public KeyboardView(bool supportsCollapsedDock = false)
        {
            //Setup binding for SupportsCollapsedDock property
            SetBinding(SupportsCollapsedDockProperty, new Binding
            {
                Path = new PropertyPath("DataContext.KeyboardSupportsCollapsedDock"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            });
            //Set SupportsCollapsedDock value on each load, which will push this value to the KeyboardHost's DataContext
            Loaded += (sender, args) => SupportsCollapsedDock = supportsCollapsedDock;
        }

        public static readonly DependencyProperty SupportsCollapsedDockProperty =
            DependencyProperty.Register("SupportsCollapsedDock", typeof(bool), typeof(KeyboardView), new PropertyMetadata(default(bool)));

        protected bool SupportsCollapsedDock
        {
            get { return (bool)GetValue(SupportsCollapsedDockProperty); }
            set { SetValue(SupportsCollapsedDockProperty, value); }
        }
    }
}
