// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Behaviours
{
    public static class KeyBehaviours
    {
        #region BeginAnimationOnKeySelectionEvent

        public static readonly DependencyProperty BeginAnimationOnKeySelectionEventProperty =
            DependencyProperty.RegisterAttached("BeginAnimationOnKeySelectionEvent", typeof (Storyboard), typeof (KeyBehaviours),
            new PropertyMetadata(default(Storyboard), BeginAnimationOnKeySelectionEventChanged));

        private static void BeginAnimationOnKeySelectionEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var storyboard = dependencyPropertyChangedEventArgs.NewValue as Storyboard;
            var frameworkElement = dependencyObject as FrameworkElement;
            var key = frameworkElement.TemplatedParent as Key;
                
            EventHandler selectionHandler = (sender, args) => storyboard.Begin(frameworkElement);
            frameworkElement.Loaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.Selection += selectionHandler;
                }
            };
            frameworkElement.Unloaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.Selection -= selectionHandler;
                }
            };
        }

        public static void SetBeginAnimationOnKeySelectionEvent(DependencyObject element, Storyboard value)
        {
            element.SetValue(BeginAnimationOnKeySelectionEventProperty, value);
        }

        public static Storyboard GetBeginAnimationOnKeySelectionEvent(DependencyObject element)
        {
            return (Storyboard)element.GetValue(BeginAnimationOnKeySelectionEventProperty);
        }

        #endregion
    }
}