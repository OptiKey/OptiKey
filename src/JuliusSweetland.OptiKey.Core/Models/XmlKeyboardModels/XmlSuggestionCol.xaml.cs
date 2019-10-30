// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System.Windows;
using System.Windows.Controls;

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

        public static readonly DependencyProperty NumberOfSuggestionsDisplayedProperty =
            DependencyProperty.Register("NumberOfSuggestionsDisplayed", typeof(int), typeof(XmlSuggestionCol), new PropertyMetadata(default(int)));

        public int NumberOfSuggestionsDisplayed
        {
            get { return (int)GetValue(NumberOfSuggestionsDisplayedProperty); }
            set { SetValue(NumberOfSuggestionsDisplayedProperty, value); }
        }
    }
}
