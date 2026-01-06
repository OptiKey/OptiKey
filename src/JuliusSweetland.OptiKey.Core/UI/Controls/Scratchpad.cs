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

        public static readonly DependencyProperty ForegroundColourOverrideProperty =
            DependencyProperty.Register("ForegroundColourOverride", typeof(Brush), typeof(Scratchpad), new PropertyMetadata(default(Brush)));

        public Brush ForegroundColourOverride
        {
            get { return (Brush)GetValue(ForegroundColourOverrideProperty); }
            set { SetValue(ForegroundColourOverrideProperty, value); }
        }

        public static readonly DependencyProperty OpacityOverrideProperty =
            DependencyProperty.Register("OpacityOverride", typeof(double), typeof(Scratchpad), new PropertyMetadata(defaultValue: 1.0));

        public double OpacityOverride
        {
            get { return (double)GetValue(OpacityOverrideProperty); }
            set { SetValue(OpacityOverrideProperty, value); }
        }

        public static readonly DependencyProperty DisabledForegroundBrushProperty =
            DependencyProperty.Register("DisabledForegroundBrush", typeof(Brush), typeof(Scratchpad), new PropertyMetadata(default(Brush)));

        public Brush DisabledForegroundBrush
        {
            get { return (Brush)GetValue(DisabledForegroundBrushProperty); }
            set { SetValue(DisabledForegroundBrushProperty, value); }
        }

        public static readonly DependencyProperty DisabledBackgroundBrushProperty =
            DependencyProperty.Register("DisabledBackgroundBrush", typeof(Brush), typeof(Scratchpad), new PropertyMetadata(default(Brush)));

        public Brush DisabledBackgroundBrush
        {
            get { return (Brush)GetValue(DisabledBackgroundBrushProperty); }
            set { SetValue(DisabledBackgroundBrushProperty, value); }
        }
    }
}
