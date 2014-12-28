using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using JuliusSweetland.ETTA.UI.Controls;

namespace JuliusSweetland.ETTA.UI.Behaviours
{
    public static class KeyBehaviours
    {
        public static readonly DependencyProperty BeginAnimationOnKeySelectionEventProperty =
            DependencyProperty.RegisterAttached("BeginAnimationOnKeySelectionEvent", typeof (Storyboard), typeof (KeyBehaviours),
            new PropertyMetadata(default(Storyboard), BeginAnimationOnKeySelectionEventChanged));

        private static void BeginAnimationOnKeySelectionEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var storyboard = dependencyPropertyChangedEventArgs.NewValue as Storyboard;
            var frameworkElement = dependencyObject as FrameworkElement;
            var key = frameworkElement != null
                ? frameworkElement.TemplatedParent as Key
                : null;
                
            if (storyboard != null
                && frameworkElement != null
                && key != null)
            {
                EventHandler selectionHandler = (sender, args) => storyboard.Begin(frameworkElement);
                frameworkElement.Loaded += (sender, args) => key.Selection += selectionHandler;
                frameworkElement.Unloaded += (sender, args) => key.Selection -= selectionHandler;
            }
        }

        public static void SetBeginAnimationOnKeySelectionEvent(DependencyObject element, Storyboard value)
        {
            element.SetValue(BeginAnimationOnKeySelectionEventProperty, value);
        }

        public static Storyboard GetBeginAnimationOnKeySelectionEvent(DependencyObject element)
        {
            return (Storyboard)element.GetValue(BeginAnimationOnKeySelectionEventProperty);
        }
    }
}
