using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class Key : UserControl, INotifyPropertyChanged
    {
        #region Private Member Vars

        private Action calculateIsEnabled = null;

        #endregion

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
            var mainViewModel = keyboardHost.DataContext as MainViewModel;
            var keyboardService = mainViewModel.KeyboardService;
            var capturingStateManager = mainViewModel.CapturingStateManager;

            //Calculate KeyDownState
            keyboardService.KeyDownStates[Value].OnPropertyChanges(kds => kds.Value)
                .Subscribe(value => KeyDownState = value);

            KeyDownState = keyboardService.KeyDownStates[Value].Value;

            //Calculate SelectionProgress
            keyboardService.KeySelectionProgress[Value].OnPropertyChanges(ksp => ksp.Value)
                .Subscribe(value => SelectionProgress = value);

            SelectionProgress = keyboardService.KeySelectionProgress[Value].Value;

            //Calculate IsEnabled
            calculateIsEnabled = () => IsEnabled = keyboardService.KeyEnabledStates[Value];
            keyboardService.KeyEnabledStates.OnAnyPropertyChanges().Subscribe(_ => calculateIsEnabled());
            calculateIsEnabled();
            
            //Calculate IsCurrent
            Action<KeyValue?> calculateIsCurrent = value => IsCurrent = value != null && value.Value.Equals(Value);

            mainViewModel.OnPropertyChanges(vm => vm.CurrentPositionKey)
                .Subscribe(calculateIsCurrent);

            calculateIsCurrent(mainViewModel.CurrentPositionKey);
            
            //Calculate DisplayShiftDownText
            Action<KeyDownStates, bool> calculateDisplayShiftDownText = 
                (shiftDownState, capturingMultiKeySelection) => 
                    DisplayShiftDownText = shiftDownState == KeyDownStates.LockedDown
                    || (shiftDownState == KeyDownStates.Down && !capturingMultiKeySelection);

            capturingStateManager.OnPropertyChanges(csm => csm.CapturingMultiKeySelection)
                .Subscribe(value => calculateDisplayShiftDownText(keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value, value));

            keyboardService.KeyDownStates[KeyValues.LeftShiftKey].OnPropertyChanges(sds => sds.Value)
                .Subscribe(value => calculateDisplayShiftDownText(value, capturingStateManager.CapturingMultiKeySelection));

            calculateDisplayShiftDownText(
                keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value,
                capturingStateManager.CapturingMultiKeySelection);

            //Publish own version of KeySelection event
            mainViewModel.KeySelection += (o, value) =>
            {
                if (value.Equals(Value)
                    && Selection != null)
                {
                    Selection(this, null);
                }
            };
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

        public static readonly DependencyProperty DisplayShiftDownTextProperty =
            DependencyProperty.Register("DisplayShiftDownText", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool DisplayShiftDownText
        {
            get { return (bool) GetValue(DisplayShiftDownTextProperty); }
            set { SetValue(DisplayShiftDownTextProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentProperty =
            DependencyProperty.Register("IsCurrent", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool IsCurrent
        {
            get { return (bool) GetValue(IsCurrentProperty); }
            set { SetValue(IsCurrentProperty, value); }
        }

        public static readonly DependencyProperty SelectionProgressProperty =
            DependencyProperty.Register("SelectionProgress", typeof(double), typeof(Key), new PropertyMetadata(default(double)));

        public double SelectionProgress
        {
            get { return (double) GetValue(SelectionProgressProperty); }
            set { SetValue(SelectionProgressProperty, value); }
        }
        
        //Specify if this key spans multiple keys horizontally - used to keep the contents proportional to other keys
        public static readonly DependencyProperty WidthSpanProperty =
            DependencyProperty.Register("WidthSpan", typeof(int), typeof(Key), new PropertyMetadata(1));

        public int WidthSpan
        {
            get { return (int) GetValue(WidthSpanProperty); }
            set { SetValue(WidthSpanProperty, value); }
        }

        //Specify if this key spans multiple keys vertically - used to keep the contents proportional to other keys
        public static readonly DependencyProperty HeightSpanProperty =
            DependencyProperty.Register("HeightSpan", typeof(int), typeof(Key), new PropertyMetadata(1));

        public int HeightSpan
        {
            get { return (int) GetValue(HeightSpanProperty); }
            set { SetValue(HeightSpanProperty, value); }
        }

        public static readonly DependencyProperty SharedSizeGroupProperty =
            DependencyProperty.Register("SharedSizeGroup", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

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
            DependencyProperty.Register("Text", typeof(string), typeof(Key), new PropertyMetadata(default(string), TextChanged));

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
