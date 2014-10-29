using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using JuliusSweetland.ETTA.Annotations;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.ViewModels;

namespace JuliusSweetland.ETTA.UI.Controls
{
    public class Key : UserControl, INotifyPropertyChanged
    {
        #region Ctor

        public Key()
        {
            Loaded += OnLoaded;
        }

        #endregion

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var keyboardHost = VisualAndLogicalTreeHelper.FindVisualParent<KeyboardHost>(this);

            var mainViewModel = keyboardHost != null
                ? keyboardHost.DataContext as MainViewModel
                : null;

            if (mainViewModel != null
                && !string.IsNullOrEmpty(Value.Key))
            {
                /* 
                 * Certain characters have meaning in property paths, for example the comma.
                 * To ensure WPF doesn't mistake the KeyValue's Key string as a special symbol
                 * they must be escaped using the caret character (as we are inside an indexer).
                 */
                this.SetBinding(KeyDownStateProperty, new Binding
                {
                    Path = new PropertyPath(string.Format("KeyDownStates[^{0}].Value", Value.Key)),
                    Source = mainViewModel
                });

                var shiftKey = new KeyValue { FunctionKey = FunctionKeys.Shift }.Key;
                this.SetBinding(ShiftDownStateProperty, new Binding
                {
                    Path = new PropertyPath(string.Format("KeyDownStates[^{0}].Value", shiftKey)),
                    Source = mainViewModel
                });

                this.SetBinding(SelectionProgressProperty, new Binding
                {
                    Path = new PropertyPath(string.Format("KeySelectionProgress[^{0}].Value", Value.Key)),
                    Source = mainViewModel
                });

                this.SetBinding(IsEnabledProperty, new Binding
                {
                    Path = new PropertyPath(string.Format("KeyEnabledStates[^{0}]", Value.Key)),
                    Source = mainViewModel
                });

                mainViewModel.OnPropertyChanges(vm => vm.CurrentPositionKey)
                    .Subscribe(value => IsCurrent = value != null && value.Value.Equals(Value));

                mainViewModel.KeySelection += (o, value) =>
                {
                    if (value.Equals(Value)
                        && Selection != null)
                    {
                        Selection(this, null);
                    }
                };
            }
        }

        #endregion

        #region Events

        public event EventHandler Selection;

        #endregion

        #region Properties

        public static readonly DependencyProperty KeyDownStateProperty =
            DependencyProperty.Register("KeyDownState", typeof(KeyDownStates), typeof(Key), new PropertyMetadata(default(KeyDownStates)));

        public KeyDownStates KeyDownState
        {
            get { return (KeyDownStates)GetValue(KeyDownStateProperty); }
            set { SetValue(KeyDownStateProperty, value); }
        }

        public static readonly DependencyProperty ShiftDownStateProperty =
            DependencyProperty.Register("ShiftDownState", typeof (KeyDownStates), typeof (Key),
                new PropertyMetadata(default(KeyDownStates), ShiftDownStateChanged));

        public KeyDownStates ShiftDownState
        {
            get { return (KeyDownStates) GetValue(ShiftDownStateProperty); }
            set { SetValue(ShiftDownStateProperty, value); }
        }

        private static void ShiftDownStateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as Key;
            var newValue = dependencyPropertyChangedEventArgs.NewValue as KeyDownStates?;
            if (key != null
                && newValue != null)
            {
                key.IsShiftDown = newValue.Value == KeyDownStates.On || newValue.Value == KeyDownStates.Lock;
            }
        }

        public static readonly DependencyProperty IsShiftDownProperty =
            DependencyProperty.Register("IsShiftDown", typeof (bool), typeof (Key), new PropertyMetadata(default(bool)));

        public bool IsShiftDown
        {
            get { return (bool) GetValue(IsShiftDownProperty); }
            set { SetValue(IsShiftDownProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentProperty =
            DependencyProperty.Register("IsCurrent", typeof (bool), typeof (Key), new PropertyMetadata(default(bool)));

        public bool IsCurrent
        {
            get { return (bool) GetValue(IsCurrentProperty); }
            set { SetValue(IsCurrentProperty, value); }
        }
        
        public static readonly DependencyProperty SelectionProgressProperty =
            DependencyProperty.Register("SelectionProgress", typeof (double), typeof (Key), new PropertyMetadata(default(double)));

        public double SelectionProgress
        {
            get { return (double) GetValue(SelectionProgressProperty); }
            set { SetValue(SelectionProgressProperty, value); }
        }
        
        //Specify if this key spans multiple keys horizontally - used to keep the contents proportional to other keys
        public static readonly DependencyProperty WidthSpanProperty =
            DependencyProperty.Register("WidthSpan", typeof (int), typeof (Key), new PropertyMetadata(1));

        public int WidthSpan
        {
            get { return (int) GetValue(WidthSpanProperty); }
            set { SetValue(WidthSpanProperty, value); }
        }

        //Specify if this key spans multiple keys vertically - used to keep the contents proportional to other keys
        public static readonly DependencyProperty HeightSpanProperty =
            DependencyProperty.Register("HeightSpan", typeof (int), typeof (Key), new PropertyMetadata(1));

        public int HeightSpan
        {
            get { return (int) GetValue(HeightSpanProperty); }
            set { SetValue(HeightSpanProperty, value); }
        }

        public static readonly DependencyProperty SharedSizeGroupProperty =
            DependencyProperty.Register("SharedSizeGroup", typeof (string), typeof (Key), new PropertyMetadata(default(string)));

        public string SharedSizeGroup
        {
            get { return (string) GetValue(SharedSizeGroupProperty); }
            set { SetValue(SharedSizeGroupProperty, value); }
        }
        
        public static readonly DependencyProperty SymbolGeometryProperty =
            DependencyProperty.Register("SymbolGeometry", typeof (Geometry), typeof (Key),
            new PropertyMetadata(default(Geometry), OnSymbolGeometryOrTextChanged));

        public Geometry SymbolGeometry
        {
            get { return (Geometry) GetValue(SymbolGeometryProperty); }
            set { SetValue(SymbolGeometryProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (Key), new PropertyMetadata(default(string), TextChanged));

        private static void TextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as Key;
            
            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;

                key.ShiftDownText = value;
                key.ShiftUpText = value;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ShiftUpTextProperty =
            DependencyProperty.Register("ShiftUpText", typeof(string), typeof(Key),
            new PropertyMetadata(default(string), OnSymbolGeometryOrTextChanged));

        public string ShiftUpText
        {
            get { return (string)GetValue(ShiftUpTextProperty); }
            set { SetValue(ShiftUpTextProperty, value); }
        }

        public static readonly DependencyProperty ShiftDownTextProperty =
            DependencyProperty.Register("ShiftDownText", typeof (string), typeof (Key),
            new PropertyMetadata(default(string), OnSymbolGeometryOrTextChanged));

        public string ShiftDownText
        {
            get { return (string) GetValue(ShiftDownTextProperty); }
            set { SetValue(ShiftDownTextProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (KeyValue), typeof (Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue Value
        {
            get { return (KeyValue) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnSymbolGeometryOrTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var senderAsKey = sender as Key;
            if (senderAsKey != null && senderAsKey.PropertyChanged != null)
            {
                senderAsKey.OnPropertyChanged("HasSymbol");
                senderAsKey.OnPropertyChanged("HasText");
            }
        }

        public bool HasSymbol { get { return SymbolGeometry != null; } }
        public bool HasText { get { return ShiftUpText != null || ShiftDownText != null; } }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
