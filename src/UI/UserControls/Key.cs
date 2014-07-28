using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.UI.UserControls
{
    public class Key : UserControl
    {
        public static readonly DependencyProperty SymbolGeometryProperty =
            DependencyProperty.Register("SymbolGeometry", typeof (Geometry), typeof (Key), new PropertyMetadata(default(Geometry)));

        public Geometry SymbolGeometry
        {
            get { return (Geometry) GetValue(SymbolGeometryProperty); }
            set { SetValue(SymbolGeometryProperty, value); }
        }

        public static readonly DependencyProperty LowerTextProperty =
            DependencyProperty.Register("LowerText", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string LowerText
        {
            get { return (string)GetValue(LowerTextProperty); }
            set { SetValue(LowerTextProperty, value); }
        }

        public static readonly DependencyProperty UpperTextProperty =
            DependencyProperty.Register("UpperText", typeof (string), typeof (Key), new PropertyMetadata(default(string)));

        public string UpperText
        {
            get { return (string) GetValue(UpperTextProperty); }
            set { SetValue(UpperTextProperty, value); }
        }

        public static readonly DependencyProperty IsPublishOnlyProperty =
            DependencyProperty.Register("IsPublishOnly", typeof (bool), typeof (Key), new PropertyMetadata(default(bool)));

        public bool IsPublishOnly
        {
            get { return (bool) GetValue(IsPublishOnlyProperty); }
            set { SetValue(IsPublishOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsModifierProperty =
            DependencyProperty.Register("IsModifier", typeof (bool), typeof (Key), new PropertyMetadata(default(bool)));

        public bool IsModifier
        {
            get { return (bool) GetValue(IsModifierProperty); }
            set { SetValue(IsModifierProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (KeyValue), typeof (Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue Value
        {
            get { return (KeyValue) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
