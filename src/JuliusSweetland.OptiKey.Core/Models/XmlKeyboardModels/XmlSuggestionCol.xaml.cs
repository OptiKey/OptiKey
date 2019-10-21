// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Models
{
    /// <summary>
    /// Interaction logic for XmlSuggestions.xaml
    /// </summary>
    public partial class XmlSuggestionCol : UserControl
    {
        public XmlSuggestionCol()
        {
            InitializeComponent();
            Loaded += (sender, args) => NumberOfSuggestionsDisplayed = 4;
        }

        public int Row { get; set; }
        public int Col { get; set; }

        public static readonly DependencyProperty NumberOfSuggestionsDisplayedProperty =
            DependencyProperty.Register("NumberOfSuggestionsDisplayed", typeof(int), typeof(XmlSuggestionCol), new PropertyMetadata(default(int)));

        public int NumberOfSuggestionsDisplayed
        {
            get { return (int)GetValue(NumberOfSuggestionsDisplayedProperty); }
            set { SetValue(NumberOfSuggestionsDisplayedProperty, value); }
        }
    }
}
