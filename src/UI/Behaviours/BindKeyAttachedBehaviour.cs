using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using JuliusSweetland.ETTA.UI.UserControls;
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.ViewModels;
using Key = JuliusSweetland.ETTA.UI.UserControls.Key;

namespace JuliusSweetland.ETTA.UI.Behaviours
{
    public static class BindKeyAttachedBehaviour
    {
        public static readonly DependencyProperty BindValueToSelectionProgressProperty =
            DependencyProperty.RegisterAttached("BindValueToSelectionProgress", typeof(bool),
                typeof(BindKeyAttachedBehaviour), new PropertyMetadata(default(bool), BindValueToSelectionProgressCallback));

        public static void SetBindValueToSelectionProgress(UIElement element, ProgressBar value)
        {
            element.SetValue(BindValueToSelectionProgressProperty, value);
        }

        public static ProgressBar GetBindValueToSelectionProgress(UIElement element)
        {
            return (ProgressBar) element.GetValue(BindValueToSelectionProgressProperty);
        }

        private static void BindValueToSelectionProgressCallback(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var progressBar = dependencyObject as ProgressBar;

            if ((bool)dependencyPropertyChangedEventArgs.NewValue
                && progressBar != null)
            {
                var key = VisualAndLogicalTreeHelper.FindVisualParent<Key>(progressBar);
                var keyboardHost = VisualAndLogicalTreeHelper.FindVisualParent<KeyboardHost>(progressBar);

                if (key != null
                    && keyboardHost != null)
                {
                    var mainViewModel = keyboardHost.DataContext as MainViewModel;

                    if (mainViewModel != null)
                    {
                        progressBar.SetBinding(RangeBase.ValueProperty, new Binding
                        {
                            Path = new PropertyPath(string.Format("KeySelectionProgress[{0}].Value", key.Value.Key)),
                            Source = mainViewModel
                        });
                    }
                }
            }
        }
    }
}
