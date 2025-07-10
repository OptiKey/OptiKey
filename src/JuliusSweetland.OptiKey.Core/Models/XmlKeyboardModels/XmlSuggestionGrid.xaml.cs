// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.UI.ViewModels;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using JuliusSweetland.OptiKey.UI.ValueConverters;

namespace JuliusSweetland.OptiKey.Models
{
    /// <summary>
    /// Interaction logic for XmlSuggestionGrid.xaml
    /// </summary>
    public partial class XmlSuggestionGrid : UserControl
    {
        private int rows;
        private int cols;

        private IMultiValueConverter convertSuggestionsToString = new SuggestionsToString();
        private IMultiValueConverter convertSuggestionsToKeyValue = new SuggestionsToKeyValue();
        private Binding suggestionsBinding =  new Binding
            {
                Path = new PropertyPath("DataContext.SuggestionService.Suggestions"),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(KeyboardHost), 1),
                Mode = BindingMode.TwoWay //This MUST be TwoWay to detect changes to the DataContext used in the binding path
            };

        public XmlSuggestionGrid(int numRows, int numCols, bool rightToLeft = false, bool bottomToTop = false)
        {
            InitializeComponent();

            this.rows = numRows;
            this.cols = numCols;

            AddRowsToGrid(rows);
            AddColsToGrid(cols);
            //can we have a special key that doesn't get upper-cased? presumably the suggestions keys don't...

            // Add keys for all the suggestions
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int i = r * cols + c;

                    // Calculate display position based on ordering
                    int displayRow = bottomToTop ? (rows - 1 - r) : r;
                    int displayCol = rightToLeft ? (cols - 1 - c) : c;

                    var newKey = new Key {
                        Value = new KeyValue(Enums.FunctionKeys.SuggestionN, i.ToString()),
                        Case = Enums.Case.None
                    };
                    newKey.SetBinding(Key.TextProperty, getStringBinding(i));
                    //newKey.SetBinding(Key.ValueProperty, getValueBinding(i));
                        
                    PlaceKeyInPosition(newKey, displayRow, displayCol, 1, 1);
                }
            }
        }

        private MultiBinding getStringBinding(int idx)
        {
            MultiBinding b = new MultiBinding { Converter = convertSuggestionsToString };

            b.Bindings.Add(suggestionsBinding);
            b.Bindings.Add(new Binding { Source = idx });

            return b;
        }

        private MultiBinding getValueBinding(int idx)
        {
            MultiBinding b = new MultiBinding { Converter = convertSuggestionsToKeyValue };

            b.Bindings.Add(suggestionsBinding);
            b.Bindings.Add(new Binding { Source = idx });

            return b;
        }

        private void AddRowsToGrid(int nRows)
        {
            for (int i = 0; i < nRows; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }        
        }

        private void AddColsToGrid(int nCols)
        {
            for (int i = 0; i < nCols; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void PlaceKeyInPosition(Key key, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            MainGrid.Children.Add(key);
            Grid.SetColumn(key, col);
            Grid.SetRow(key, row);
            Grid.SetColumnSpan(key, colSpan);
            Grid.SetRowSpan(key, rowSpan);
        }
    }
}
