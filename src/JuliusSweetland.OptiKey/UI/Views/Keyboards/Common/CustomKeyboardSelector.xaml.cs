using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Enums;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Reflection;

using log4net;


namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{

    /// <summary>
    /// Interaction logic for CustomKeyboardSelector.xaml
    /// </summary>
    public partial class CustomKeyboardSelector : KeyboardView
    {

        #region Private Members
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private CustomKeyboardFolder folder;
        private int pageIndex = 0;

        // TODO: Make user configurable?
        private int mRows = 3;
        private int mCols = 4;

        #endregion

        public CustomKeyboardSelector(int pageIndex)
        {
            InitializeComponent();
            this.pageIndex = pageIndex;

            // Populate model
            folder = new CustomKeyboardFolder();
            
            // Setup grid
            for (int i = 0; i < this.mRows; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < this.mCols; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Add back key 
            { 
                Key newKey = new Key();
                newKey.SharedSizeGroup = "BackButton";
                newKey.SymbolGeometry = (System.Windows.Media.Geometry)App.Current.Resources["BackIcon"];
                newKey.Text = JuliusSweetland.OptiKey.Properties.Resources.BACK;
                newKey.Value = KeyValues.BackFromKeyboardKey;
                this.AddKey(newKey, this.mRows - 1, this.mCols - 1);
            }

            // Empty key for bottom left
            {
                Key newKey = new Key();
                newKey.SharedSizeGroup = "BackButton";
                this.AddKey(newKey, this.mRows - 1, 0);
            }

            // Add keyboard keys, or blanks
            int maxKeyboardsPerPage = (this.mCols - 2) * this.mRows;
            int totalNumKeyboards = folder.keyboards.Count;
            int remainingKeyboards =  totalNumKeyboards - maxKeyboardsPerPage*pageIndex;
            int nKBs = Math.Min(remainingKeyboards, maxKeyboardsPerPage);
            int firstKB = maxKeyboardsPerPage * pageIndex;
            for (int i = 0; i < maxKeyboardsPerPage; i++)
            {
                var r = i / (this.mCols - 2); // integer division
                var c = 1 + (i % (this.mCols - 2)); 

                if (i < nKBs)
                {
                    KeyboardInfo kbInfo = folder.keyboards.ElementAt(firstKB + i);
                    // Add key to link to keyboard
                    this.AddKeyboardKey(kbInfo, r, c);
                }
                else
                {
                    // Add empty/inactive key for consistent aesthetic
                    Key newKey = new Key();
                    if (i == 0)
                    {
                        // Special case for empty grid                       
                        newKey.SharedSizeGroup = "BackButton";
                        newKey.Text = "No keyboards\nfound"; // TODO: resource string   
                    }
                    this.AddKey(newKey, r, c);
                }
            }

            // Add prev/next keys (may be disabled)
            {
                Key newKey = new Key();
                newKey.SharedSizeGroup = "BackButton";        
                newKey.SymbolGeometry = (System.Windows.Media.Geometry)App.Current.Resources["ArrowPointingToLeftIcon"];
                newKey.Text = "Prev"; // TODO: resource string
                if (this.pageIndex > 0)
                {
                    newKey.Value = KeyValues.CustomKeyboardPrev;
                }
                this.AddKey(newKey, 0, 0, 2, 1);
            }
            {
                Key newKey = new Key();
                newKey.SharedSizeGroup = "BackButton";
                newKey.SymbolGeometry = (System.Windows.Media.Geometry)App.Current.Resources["ArrowPointingToRightIcon"];
                newKey.Text = "Next"; // TODO: resource string
                if (nKBs < remainingKeyboards)
                {
                    newKey.Value = KeyValues.CustomKeyboardNext;
                }
                this.AddKey(newKey, 0, this.mCols - 1, 2, 1);
            }
        }

        private void AddKey(Key key, int row, int col, int rowspan = 1, int colspan = 1)
        {
            MainGrid.Children.Add(key);
            Grid.SetColumn(key, col);
            Grid.SetRow(key, row);
            Grid.SetColumnSpan(key, colspan);
            Grid.SetRowSpan(key, rowspan);

        }

        private void AddKeyboardKey(KeyboardInfo info, int row, int col)
        {
            Key lKey = new Key();
            lKey.Value = new KeyValueLink(info.fullPath);
            lKey.SharedSizeGroup = "KeyboardKey";
            lKey.Text = info.keyboardName;
            if (info.symbolString != null)
            {
                Geometry geom = (Geometry)this.Resources[info.symbolString];
                if (null != geom)
                {
                    lKey.SymbolGeometry = geom;
                }
            }
            this.AddKey(lKey, row, col);
        }

        private int RowFromIndex(int index)
        {
            return index / this.mCols; // integer division
        }

        private int ColFromIndex(int index)
        {
            return index % this.mCols;
        }

    }
}
