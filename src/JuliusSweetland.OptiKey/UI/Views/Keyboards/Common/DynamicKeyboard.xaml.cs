using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Extensions;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Linq;
using System.Windows.Media;
using System.Reflection;
using log4net;
using System.Windows;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for DynamicKeyboard.xaml
    /// </summary>
    public partial class DynamicKeyboard : KeyboardView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string inputFilename;
        private XmlKeyboard keyboard;

        public DynamicKeyboard(string inputFile, Action<double> resizeAction)
        {

            InitializeComponent();
            inputFilename = inputFile;
            
            // Read in XML file, exceptions get displayed to user
            try
            {
                keyboard = XmlKeyboard.ReadFromFile(inputFilename);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                SetupErrorLayout("Error loading file", SplitAndWrapExceptionInfo(e.ToString()));
                return;
            }

            if (ValidateKeyboard()) {
                // Setup all the UI components      
                SetupGrid();
                SetupKeys();

                // Apply size changes after any optikey general sizing policies.
                this.SizeChanged += (_, args) =>
                {
                    SetupKeyboardLayout(resizeAction);
                };
            }
        }

        private bool ValidateKeyboard()
        {
            if (keyboard.Grid == null)
            {
                SetupErrorLayout("Invalid keyboard file", "No grid definition found");
                return false;
            }

            if (keyboard.Keys == null || keyboard.Keys.Count == 0)
            {
                SetupErrorLayout("Invalid keyboard file", "No key definitions found");
                return false;
            }

            if (keyboard.Grid.Rows < 1 || keyboard.Grid.Cols < 1)
            {
                string content = "Grid size is " + keyboard.Grid.Rows + " rows and " 
                    + keyboard.Grid.Cols + " columns";
                SetupErrorLayout("Incorrect grid definition", content);
                return false;
            }

            return true;
        }

        private Key CreateKeyWithBasicProps(XmlKey xmlKey)
        {
            // Add the core properties from XML to a new key
            Key newKey = new Key();
            newKey.Text = xmlKey.Label.ToStringWithValidNewlines();

            if (null != xmlKey.Symbol)
            {
                Geometry geom = (Geometry)this.Resources[xmlKey.Symbol];
                if (null != geom)
                {
                    newKey.SymbolGeometry = geom;
                }
                else
                {
                    Log.ErrorFormat("Could not parse {0} as symbol geometry", xmlKey.Symbol);
                }
            }

            // Add same symbol margin to all keys
            if (keyboard.SymbolMargin.HasValue)
            {
                newKey.SymbolMargin = keyboard.SymbolMargin.Value;
            }

            // Set shared size group
            bool hasSymbol = null  != newKey.SymbolGeometry;
            bool hasString = xmlKey.Label.Length > 0;
            if (hasSymbol && hasString)
            {
                newKey.SharedSizeGroup = "KeyWithSymbolAndText";
            }
            else if (hasSymbol)
            {
                newKey.SharedSizeGroup = "KeyWithSymbol";            
            }
            else if (hasString)
            {
                newKey.SharedSizeGroup = "KeyWithText";
            }
            // Also group separately by row/col width/height
            newKey.SharedSizeGroup += xmlKey.Width;
            newKey.SharedSizeGroup += xmlKey.Height;

            return newKey;
        }

        private string SplitAndWrapExceptionInfo(string info)
        {
            // Take first line of error message
            info = info.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)[0];

            // Wrap to (approx) three lines
            var len = info.Length;           
            var maxLineLength = len/3.5;
            Log.Info(maxLineLength);
            char[] space = new char[] { ' ' };
            
            var charCount = 0;
            var allLines = info.Split(space)
                .GroupBy(w =>  (int)((charCount += w.Length + 1) / maxLineLength))
                .Select(g => string.Join(" ", g));

            return String.Join(Environment.NewLine, allLines);
        }

        private void SetupErrorLayout(string heading, string content)
        {

            AddRowsToGrid(4);
            AddColsToGrid(4);

            // Top middle two cells are main error message
            {
                Key newKey = new Key();
                newKey.Text = heading;
                this.PlaceKeyInPosition(newKey, 0, 1, 1, 2);
            }

            // Middle row is detailed error message
            {
                Key newKey = new Key();
                newKey.Text = content;
                this.PlaceKeyInPosition(newKey, 1, 0, 2, 4);
            }

            // Back key
            {
                Key newKey = new Key();
                newKey.SymbolGeometry = (System.Windows.Media.Geometry) App.Current.Resources["BackIcon"];
                newKey.Text = JuliusSweetland.OptiKey.Properties.Resources.BACK;
                newKey.Value = KeyValues.BackFromKeyboardKey;
                this.PlaceKeyInPosition(newKey, 3, 3);
            }

            // Fill in empty keys
            {
                Key newKey = new Key();
                this.PlaceKeyInPosition(newKey, 0, 0, 1, 1);
            }
            {
                Key newKey = new Key();
                this.PlaceKeyInPosition(newKey, 0, 3, 1, 1);
            }
            {
                Key newKey = new Key();
                this.PlaceKeyInPosition(newKey, 3, 0, 1, 1);
            }
            {
                Key newKey = new Key();
                this.PlaceKeyInPosition(newKey, 3, 1, 1, 2);
            }
        }
        

        private void SetupKeys()
        {
            XmlKeys keys = keyboard.Keys;
            
            // Iterate over each possible type of key and add to keyboard
            foreach (XmlActionKey key in keys.ActionKeys)
            {
                AddActionKey(key);
            }

            foreach (XmlTextKey key in keys.TextKeys)
            {
                AddTextKey(key);
            }

            foreach (XmlChangeKeyboardKey key in keys.ChangeKeyboardKeys)
            {
                AddChangeKeyboardKey(key);
            }
        }

        void AddChangeKeyboardKey(XmlChangeKeyboardKey xmlKey)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey);

            if (null != xmlKey.DestinationKeyboard)
            {
                var rootDir = Path.GetDirectoryName(inputFilename);
                bool replaceCurrKeyboard = !xmlKey.ReturnToThisKeyboard;
                newKey.Value = new ChangeKeyboardKeyValue(Path.Combine(rootDir, xmlKey.DestinationKeyboard), replaceCurrKeyboard);
            }
            else
            {
                Log.ErrorFormat("No destination keyboard found for changekeyboard key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);            
        }

        void AddTextKey(XmlTextKey xmlKey)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey);

            if (null != xmlKey.Text)
            {
                newKey.Value = new KeyValue(xmlKey.Text);
            }
            else
            {
                Log.ErrorFormat("No value found in text key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);            
        }

        void AddActionKey(XmlActionKey xmlKey)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey);

            if (xmlKey.Action.HasValue)
            {
                newKey.Value = new KeyValue(xmlKey.Action.Value);
            }
            else { 
                Log.ErrorFormat("No FunctionKey found for key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        private void SetupKeyboardLayout(Action<double> resizeAction)
        {
            // Get height, if specified, resize window.
            if (keyboard.Height.HasValue)
            {
                if (keyboard.Height > 100 || keyboard.Height < 1)
                {
                    SetupErrorLayout("Incorrect height", "Must be in range [1, 100] (as percent of screen height)");
                }
                else
                {
                    resizeAction(keyboard.Height.Value);
                }
            }

            // Get border thickness, if specified, to override
            if (keyboard.BorderThickness.HasValue)
            {
                Log.InfoFormat("Setting border thickness for custom keyboard: {0}",
                               keyboard.BorderThickness.Value);
                this.BorderThickness = keyboard.BorderThickness.Value;
            }
        }

        private void SetupGrid()
        {
            XmlGrid grid = keyboard.Grid;
            AddRowsToGrid(grid.Rows);
            AddColsToGrid(grid.Cols);
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

        private void PlaceKeyInPosition(Key key, int row, int col, int rowspan = 1, int colspan = 1)
        {         
            MainGrid.Children.Add(key);
            Grid.SetColumn(key, col);
            Grid.SetRow(key, row);
            Grid.SetColumnSpan(key, colspan);
            Grid.SetRowSpan(key, rowspan);
        }        
    }
}
