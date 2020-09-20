// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.UI.Behaviours
{
    public static class TextBoxBehaviours
    {
        public static readonly DependencyProperty CaretElementProperty =
            DependencyProperty.RegisterAttached("CaretElement", typeof(FrameworkElement), typeof(TextBoxBehaviours),
            new PropertyMetadata(default(FrameworkElement), CaretElementChanged));

        private static void CaretElementChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is TextBox textBox
                && dependencyPropertyChangedEventArgs.NewValue is FrameworkElement caretElement)
            {
                CompositeDisposable compositeDisposable = null;
                textBox.Loaded += (_, __) =>
                {
                    PositionCaret(textBox, caretElement);

                    compositeDisposable = new CompositeDisposable
                    {
                        textBox.OnPropertyChanges<string>(TextBox.TextProperty).Subscribe(___ => PositionCaret(textBox, caretElement)),
                        textBox.OnPropertyChanges<double>(Control.FontSizeProperty).Subscribe(___ => PositionCaret(textBox, caretElement)),
                        textBox.OnPropertyChanges<double>(FrameworkElement.ActualWidthProperty).Subscribe(___ => PositionCaret(textBox, caretElement)),
                        textBox.OnPropertyChanges<double>(FrameworkElement.ActualHeightProperty).Subscribe(___ => PositionCaret(textBox, caretElement))
                    };
                };

                textBox.Unloaded += (_, __) =>
                {
                    compositeDisposable?.Dispose();
                    compositeDisposable = null;
                };
            }
        }

        public static void SetCaretElement(UIElement element, string value)
        {
            element.SetValue(CaretElementProperty, value);
        }

        public static string GetCaretElement(UIElement element)
        {
            return (string) element.GetValue(CaretElementProperty);
        }

        private static void PositionCaret(TextBox textBox, UIElement caretElement)
        {
            var caretLocation = textBox.GetRectFromCharacterIndex(textBox.Text.Length).Location;

            if (!double.IsInfinity(caretLocation.X))
            {
                Canvas.SetLeft(caretElement, caretLocation.X);
            }

            if (!double.IsInfinity(caretLocation.Y))
            {
                Canvas.SetTop(caretElement, caretLocation.Y);
            }
        }
    }
}
