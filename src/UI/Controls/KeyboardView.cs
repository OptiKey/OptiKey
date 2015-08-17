using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public abstract class KeyboardView : UserControl
    {
        public static readonly DependencyProperty KeyboardSupportsCollapsedDockProperty =
            DependencyProperty.Register("KeyboardSupportsCollapsedDock", typeof(bool), typeof(KeyboardView), new PropertyMetadata(default(bool)));

        public bool KeyboardSupportsCollapsedDock
        {
            get { return (bool)GetValue(KeyboardSupportsCollapsedDockProperty); }
            set { SetValue(KeyboardSupportsCollapsedDockProperty, value); }
        }
    }
}
