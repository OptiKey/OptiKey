// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.UI.ViewModels;

namespace JuliusSweetland.OptiKey.UI.Behaviours
{
    public static class CursorBehaviours
    {
        public static readonly DependencyProperty BeginAnimationOnPointSelectionEventProperty =
            DependencyProperty.RegisterAttached("BeginAnimationOnPointSelectionEvent", typeof(Storyboard), typeof(CursorBehaviours),
            new PropertyMetadata(default(Storyboard), BeginAnimationOnPointSelectionEventChanged));

        private static void BeginAnimationOnPointSelectionEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var storyboard = dependencyPropertyChangedEventArgs.NewValue as Storyboard;
            var frameworkElement = dependencyObject as FrameworkElement;
            var cursor = frameworkElement.TemplatedParent as Cursor;

            if (cursor != null)
            {
                var mainViewModel = cursor.DataContext as MainViewModel;

                EventHandler<Point> selectionHandler = (sender, point) =>
                {
                    if (storyboard != null && frameworkElement.IsLoaded && frameworkElement.IsVisible)
                    {
                        storyboard.Begin(frameworkElement);
                    }
                };
                frameworkElement.Loaded += (sender, args) =>
                {
                    if (mainViewModel != null)
                    {
                        mainViewModel.PointSelection += selectionHandler;
                    }
                };

                frameworkElement.Unloaded += (sender, args) =>
                {
                    if (mainViewModel != null)
                    {
                        mainViewModel.PointSelection -= selectionHandler;
                    }
                };
            }
        }

        public static void SetBeginAnimationOnPointSelectionEvent(DependencyObject element, Storyboard value)
        {
            element.SetValue(BeginAnimationOnPointSelectionEventProperty, value);
        }

        public static Storyboard GetBeginAnimationOnPointSelectionEvent(DependencyObject element)
        {
            return (Storyboard)element.GetValue(BeginAnimationOnPointSelectionEventProperty);
        }
    }
}
