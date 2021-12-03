// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Behaviours
{
    public static class ScrollViewerBehaviours
    {
        public static readonly DependencyProperty AutoScrollToEndOnPropertyOrCollectionChangedProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEndOnPropertyOrCollectionChanged",
            typeof(object), typeof(ScrollViewerBehaviours), new PropertyMetadata(default(object), AutoScrollToEndOnPropertyOrCollectionChangedCallback));

        public static void SetAutoScrollToEndOnPropertyOrCollectionChanged(ScrollViewer element, object value)
        {
            element.SetValue(AutoScrollToEndOnPropertyOrCollectionChangedProperty, value);
        }

        public static object GetAutoScrollToEndOnPropertyOrCollectionChanged(ScrollViewer element)
        {
            return element.GetValue(AutoScrollToEndOnPropertyOrCollectionChangedProperty);
        }

        private static void AutoScrollToEndOnPropertyOrCollectionChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var scrollViewer = dependencyObject as ScrollViewer;
            var notifyPropertyChanged = dependencyPropertyChangedEventArgs.NewValue as INotifyPropertyChanged;
            var notifyCollectionChanged = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;

            if (scrollViewer != null)
            {
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged += (sender, args) => scrollViewer.ScrollToEnd();
                }

                if (notifyCollectionChanged != null)
                {
                    notifyCollectionChanged.CollectionChanged += (sender, args) => scrollViewer.ScrollToEnd();
                }

                scrollViewer.ScrollToEnd();
            }
        }
    }
}
