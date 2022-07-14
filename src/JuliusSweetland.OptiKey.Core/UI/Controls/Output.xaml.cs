// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for Output.xaml
    /// </summary>
    public partial class Output : UserControl
    {
        public Output()
        {
            InitializeComponent();
            Loaded += (sender, args) => {
                BindableNumberOfSuggestionsDisplayed = NumberOfSuggestionsDisplayed;
                ShowSuggestionsKey5 = BindableNumberOfSuggestionsDisplayed >= 5 ? true : false;
                ShowSuggestionsKey6 = BindableNumberOfSuggestionsDisplayed >= 6 ? true : false;
            };
        }

        public int NumberOfSuggestionsDisplayed { get; set; } = 4;
        public Boolean ShowSuggestionsKey5 { get; set; } = false;
        public Boolean ShowSuggestionsKey6 { get; set; } = false;

        public static readonly DependencyProperty BindableNumberOfSuggestionsDisplayedProperty =
            DependencyProperty.Register("BindableNumberOfSuggestionsDisplayed", typeof (int), typeof (Output), new PropertyMetadata(default(int)));

        public int BindableNumberOfSuggestionsDisplayed
        {
            get { return (int) GetValue(BindableNumberOfSuggestionsDisplayedProperty); }
            set { SetValue(BindableNumberOfSuggestionsDisplayedProperty, value); }
        }

        public static readonly DependencyProperty ScratchpadWidthInKeysProperty = DependencyProperty.Register(
            "ScratchpadWidthInKeys", typeof (int), typeof (Output), new PropertyMetadata(default(int)));

        public int ScratchpadWidthInKeys
        {
            get { return (int) GetValue(ScratchpadWidthInKeysProperty); }
            set { SetValue(ScratchpadWidthInKeysProperty, value); }
        }
    }
}
