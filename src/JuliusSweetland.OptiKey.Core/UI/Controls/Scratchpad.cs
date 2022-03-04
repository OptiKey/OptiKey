// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class Scratchpad : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Scratchpad), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColourOverrideProperty =
            DependencyProperty.Register("BackgroundColourOverride", typeof(Brush), typeof(Scratchpad), new PropertyMetadata(default(Brush)));

        public Brush BackgroundColourOverride
        {
            get { return (Brush)GetValue(BackgroundColourOverrideProperty); }
            set { SetValue(BackgroundColourOverrideProperty, value); }
        }

        public static readonly DependencyProperty OpacityOverrideProperty =
            DependencyProperty.Register("OpacityOverride", typeof(double), typeof(Scratchpad), new PropertyMetadata(defaultValue: 1.0));

        public double OpacityOverride
        {
            get { return (double)GetValue(OpacityOverrideProperty); }
            set { SetValue(OpacityOverrideProperty, value); }
        }
    }
}
