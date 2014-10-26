using System;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Extensions;

namespace JuliusSweetland.ETTA.UI.Behaviours
{
    public static class TextBoxBehaviours
    {
        public static readonly DependencyProperty CaretElementProperty =
            DependencyProperty.RegisterAttached("CaretElement", typeof(UIElement), typeof(TextBoxBehaviours),
            new PropertyMetadata(default(UIElement), CaretElementChanged));

        private static void CaretElementChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBox = dependencyObject as TextBox;
            var caretElement = dependencyPropertyChangedEventArgs.NewValue as UIElement;

            if (textBox != null
                && caretElement != null)
            {
                PositionCaret(textBox, caretElement);

                textBox.OnPropertyChanges<string>(TextBox.TextProperty).Subscribe(_ => PositionCaret(textBox, caretElement));
                textBox.OnPropertyChanges<double>(Control.FontSizeProperty).Subscribe(_ => PositionCaret(textBox, caretElement));
                textBox.OnPropertyChanges<double>(Control.ActualWidthProperty).Subscribe(_ => PositionCaret(textBox, caretElement));
                textBox.OnPropertyChanges<double>(Control.ActualHeightProperty).Subscribe(_ => PositionCaret(textBox, caretElement));
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
