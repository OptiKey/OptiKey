using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels;

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

        #region IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueTo

        public static readonly DependencyProperty IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueToProperty =
            DependencyProperty.RegisterAttached("IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueTo", typeof(KeyValue), typeof(KeyBehaviours),
            new PropertyMetadata(default(KeyValue), IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueToChanged));

        private static void IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueToChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var trailingConsonantOrFinalJamo = (KeyValue)dependencyPropertyChangedEventArgs.NewValue;
            var key = dependencyObject as Key;
            var defaultKeyValue = key.Value;

            IDisposable lastCharacterSubscription = null;
            key.Loaded += (sender, args) =>
            {
                var keyboardHost = VisualAndLogicalTreeHelper.FindVisualParent<KeyboardHost>(key);
                var mainViewModel = keyboardHost.DataContext as MainViewModel;
                lastCharacterSubscription = mainViewModel.KeyboardOutputService.OnPropertyChanges(kos => kos.Text).Subscribe(t =>
                {
                    if (string.IsNullOrEmpty(t)) return;

                    var lastChar = t.Last();
                    if (lastChar.ToUnicodeCodePointRange() == UnicodeCodePointRanges.HangulSyllable)
                    {
                        var composedChar = lastChar.ToString();
                        var decomposedChar = composedChar.Decompose();
                        if (decomposedChar != null
                            && decomposedChar != composedChar
                            && decomposedChar.Last().ToUnicodeCodePointRange() == UnicodeCodePointRanges.HangulVowelOrMedialJamo)
                        {
                            key.Value = trailingConsonantOrFinalJamo;
                            return;
                        }
                    }

                    key.Value = defaultKeyValue;
                });
            };
            key.Unloaded += (sender, args) =>
            {
                if (lastCharacterSubscription != null)
                {
                    lastCharacterSubscription.Dispose();
                }
            };
        }

        public static void SetIfPreviousCharIsKoreanMedialJamoThenChangeKeyValueTo(DependencyObject element, KeyValue value)
        {
            element.SetValue(IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueToProperty, value);
        }

        public static KeyValue GetIfPreviousCharIsKoreanMedialJamoThenChangeKeyValueTo(DependencyObject element)
        {
            return (KeyValue)element.GetValue(IfPreviousCharIsKoreanMedialJamoThenChangeKeyValueToProperty);
        }

        #endregion
    }
}