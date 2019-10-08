// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.Controls;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Enums;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Linq;
using System.Windows.Media;
using System.Reflection;
using log4net;
using System.Xml;
using System.Windows;
using System.Text.RegularExpressions;

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

        public DynamicKeyboard(string inputFile)
        {
            InitializeComponent();
            inputFilename = inputFile;

            // Read in XML file, exceptions get displayed to user
            if (string.IsNullOrEmpty(inputFilename))
            {
                Log.Error("No file specified for dynamic keyboard");
                SetupErrorLayout("Error loading file", "No file specified. Please choose a startup file in Management Console.");
                return;
            }
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

            if (ValidateKeyboard())
            {
                // Setup all the UI components      
                SetupGrid();
                SetupKeys();
                SetupStyle();
            }
        }

        private bool ValidateKeyboard()
        {
            string errorMessage = null;
            double validNumber;
            WindowStates validWindowState;
            MoveToDirections validPosition;
            DockSizes validDockSize;
            if (!string.IsNullOrWhiteSpace(keyboard.WindowState) && Enum.TryParse(keyboard.WindowState, out validWindowState) 
                && validWindowState != WindowStates.Docked && validWindowState != WindowStates.Floating && validWindowState != WindowStates.Maximised)
                errorMessage = "WindowState not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.Position) && !Enum.TryParse<MoveToDirections>(keyboard.Position, out validPosition))
                errorMessage = "Position not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.DockSize) && !Enum.TryParse<DockSizes>(keyboard.DockSize, out validDockSize))
                errorMessage = "DockSize not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.Width) &&
                !(double.TryParse(keyboard.Width.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                errorMessage = "Width must be between -9999 and 9999";
            else if (!string.IsNullOrWhiteSpace(keyboard.Height) &&
                !(double.TryParse(keyboard.Height.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                errorMessage = "Height must be between -9999 and 9999";
            else if (!string.IsNullOrWhiteSpace(keyboard.HorizontalOffset) &&
                !(double.TryParse(keyboard.HorizontalOffset.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                errorMessage = "Offset must be between -9999 and 9999";
            else if (!string.IsNullOrWhiteSpace(keyboard.VerticalOffset) &&
                !(double.TryParse(keyboard.VerticalOffset.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                errorMessage = "Offset must be between -9999 and 9999";

            if (errorMessage != null)
            {
                SetupErrorLayout("Invalid keyboard file", errorMessage);
                return false;
            }

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

            return ValidateRowsAndColumns();
        }

        private bool ValidateRowsAndColumns()
        {
            var allKeys = keyboard.Keys.ActionKeys.Cast<XmlKey>()
                .Concat(keyboard.Keys.ChangeKeyboardKeys)
                .Concat(keyboard.Keys.DynamicKeys)
                .Concat(keyboard.Keys.OutputKeys)
                .Concat(keyboard.Keys.PluginKeys)
                .Concat(keyboard.Keys.TextKeys)
                .ToList();

            var duplicates = allKeys
                .GroupBy(key => new Tuple<int, int>(key.Row, key.Col))
                .Where(group => group.Count() > 1)
                .Select(group => group.ToList())
                .ToList();

            if (duplicates.Count == 0)
                return true;

            var errorMsg = duplicates.Select(keys =>
            {
                var keyStrings = keys.Select(key => GetKeyString(key)).Aggregate((seq, next) => $"{seq}, {next}");
                return $"{keyStrings} ({keys.First().Row}, {keys.First().Col})";
            })
                .Aggregate((msg, key) => $"{msg}, {key}");

            SetupErrorLayout("Duplicate row/column values for keys", errorMsg);
            return false;
        }

        private string GetKeyString(XmlKey xmlKey)
        {
            var textKey = xmlKey as XmlTextKey;
            if (textKey != null)
                return textKey.Text;

            return xmlKey.Label ?? xmlKey.Symbol;
        }

        private Key CreateKeyWithBasicProps(XmlKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            // Add the core properties from XML to a new key
            Key newKey = new Key();
            if (xmlKey.ShiftDownLabel != null && xmlKey.ShiftUpLabel != null)
            {
                newKey.ShiftUpText = xmlKey.ShiftUpLabel.ToStringWithValidNewlines();
                newKey.ShiftDownText = xmlKey.ShiftDownLabel.ToStringWithValidNewlines();
            }
            else if (xmlKey.Label != null)
            {
                string vLabel = xmlKey.Label.ToString();
                string vText;
                string vLookup;
                while (vLabel.Contains("{Resource:"))
                {
                    vText = vLabel.Substring(vLabel.IndexOf("{Resource:"), vLabel.IndexOf("}", vLabel.IndexOf("{Resource:")) - vLabel.IndexOf("{Resource:") + 1);
                    vLookup = Properties.Resources.ResourceManager.GetString(vText.Substring(10, vText.Length - 11).Trim());
                    vLabel = vLabel.Replace(vText, vLookup);
                }
                while (vLabel.Contains("{Setting:"))
                {
                    vText = vLabel.Substring(vLabel.IndexOf("{Setting:"), vLabel.IndexOf("}", vLabel.IndexOf("{Setting:")) - vLabel.IndexOf("{Setting:") + 1);
                    vLookup = Properties.Settings.Default[vText.Substring(9, vText.Length - 10).Trim()].ToString();
                    vLabel = vLabel.Replace(vText, vLookup);
                }

                newKey.Text = vLabel.ToStringWithValidNewlines();
            }
            else if (xmlKey.Label != null)
            {
                newKey.Text = xmlKey.Label.ToStringWithValidNewlines();
            }

            if (xmlKey.Symbol != null)
            {
                Geometry geom = (Geometry)this.Resources[xmlKey.Symbol];
                if (geom != null)
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
            if (!string.IsNullOrEmpty(xmlKey.SharedSizeGroup))
            {
                newKey.SharedSizeGroup = xmlKey.SharedSizeGroup;
            }
            else
            {
                bool hasSymbol = newKey.SymbolGeometry != null;
                bool hasString = xmlKey.Label != null || xmlKey.ShiftUpLabel != null || xmlKey.ShiftDownLabel != null;
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
                    var text = newKey.Text != null ? newKey.Text.Compose()
                        : newKey.ShiftDownText != null ? newKey.ShiftDownText.Compose()
                        : newKey.ShiftUpText?.Compose();

                    //Strip out circle character used to show diacritic marks
                    text = text?.Replace("\x25CC", string.Empty);

                    newKey.SharedSizeGroup = text != null && text.Length > 1 ? "KeyWithText" : "KeyWithSingleLetter";
                }
            }

            //Auto set width span and height span
            if (xmlKey.AutoScaleToOneKeyWidth)
            {
                newKey.WidthSpan = (double)xmlKey.Width / (double)minKeyWidth;
            }

            if (xmlKey.AutoScaleToOneKeyHeight)
            {
                newKey.HeightSpan = (double)xmlKey.Height / (double)minKeyHeight;
            }

            newKey.UsePersianCompatibilityFont = xmlKey.UsePersianCompatibilityFont;
            newKey.UseUnicodeCompatibilityFont = xmlKey.UseUnicodeCompatibilityFont;
            newKey.UseUrduCompatibilityFont = xmlKey.UseUrduCompatibilityFont;

            if (!string.IsNullOrEmpty(xmlKey.BackgroundColor)
               && (Regex.IsMatch(xmlKey.BackgroundColor, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$")
                   || System.Drawing.Color.FromName(xmlKey.BackgroundColor).IsKnownColor))
            {
                newKey.BackgroundColourOverride = (SolidColorBrush)new BrushConverter().ConvertFrom(xmlKey.BackgroundColor);
            }

            return newKey;
        }

        private string SplitAndWrapExceptionInfo(string info)
        {
            // Take first line of error message
            info = info.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)[0];

            // Wrap to (approx) three lines
            var len = info.Length;
            var maxLineLength = len / 3.5;
            Log.Info(maxLineLength);
            char[] space = new char[] { ' ' };

            var charCount = 0;
            var allLines = info.Split(space)
                .GroupBy(w => (int)((charCount += w.Length + 1) / maxLineLength))
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
                newKey.SymbolGeometry = (System.Windows.Media.Geometry)App.Current.Resources["BackIcon"];
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

            var allKeys = keys.ActionKeys.Cast<XmlKey>()
                .Concat(keys.ChangeKeyboardKeys.Cast<XmlKey>())
                .Concat(keys.DynamicKeys.Cast<XmlKey>())
                .Concat(keys.OutputKeys.Cast<XmlKey>())
                .Concat(keys.PluginKeys.Cast<XmlKey>())
                .Concat(keys.TextKeys.Cast<XmlKey>())
                .ToList();

            var minKeyWidth = allKeys.Select(k => k.Width).Min();
            var minKeyHeight = allKeys.Select(k => k.Height).Min();

            // Iterate over each possible type of key and add to keyboard
            foreach (XmlActionKey key in keys.ActionKeys)
            {
                AddActionKey(key, minKeyWidth, minKeyHeight);
            }

            foreach (XmlChangeKeyboardKey key in keys.ChangeKeyboardKeys)
            {
                AddChangeKeyboardKey(key, minKeyWidth, minKeyHeight);
            }

            foreach (XmlDynamicKey key in keys.DynamicKeys)
            {
                AddDynamicKey(key, minKeyWidth, minKeyHeight);
            }

            foreach (XmlOutputKey key in keys.OutputKeys)
            {
                AddOutputKey(key, minKeyWidth, minKeyHeight);
            }

            foreach (XmlPluginKey key in keys.PluginKeys)
            {
                AddPluginKey(key, minKeyWidth, minKeyHeight);
            }

            foreach (XmlTextKey key in keys.TextKeys)
            {
                AddTextKey(key, minKeyWidth, minKeyHeight);
            }
        }

        void AddActionKey(XmlActionKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);

            if (xmlKey.Action.HasValue)
            {
                newKey.Value = new KeyValue(xmlKey.Action.Value);
            }
            else
            {
                Log.ErrorFormat("No FunctionKey found for key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        void AddChangeKeyboardKey(XmlChangeKeyboardKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);

            if (xmlKey.DestinationKeyboard != null)
            {
                var rootDir = Path.GetDirectoryName(inputFilename);
                bool replaceCurrKeyboard = !xmlKey.ReturnToThisKeyboard;
                Enums.Keyboards keyboardEnum;
                newKey.Value = System.Enum.TryParse(xmlKey.DestinationKeyboard, out keyboardEnum)
                    ? new ChangeKeyboardKeyValue(keyboardEnum, replaceCurrKeyboard)
                    : new ChangeKeyboardKeyValue(Path.Combine(rootDir, xmlKey.DestinationKeyboard), replaceCurrKeyboard);
            }
            else
            {
                Log.ErrorFormat("No destination keyboard found for changekeyboard key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        void AddDynamicKey(XmlDynamicKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);
            if (xmlKey.Steps != null)
            {
                var vStepList = "<StepList>";
                var rootDir = Path.GetDirectoryName(inputFilename);
                string vDestinationKeyboard;
                Enums.Keyboards keyboardEnum;

                foreach (var vStep in xmlKey.Steps)
                {
                    if (!String.IsNullOrWhiteSpace(vStep.Action))
                    {
                        vStepList += "<Action>" + vStep.Action;
                    }
                    if (!String.IsNullOrWhiteSpace(vStep.DestinationKeyboard))
                    {
                        vDestinationKeyboard = System.Enum.TryParse(vStep.DestinationKeyboard, out keyboardEnum)
                            ? keyboardEnum.ToString()
                            : Path.Combine(rootDir, vStep.DestinationKeyboard);

                        vStepList += (vStep.ReturnToThisKeyboard)
                            ? "<KeyboardAndReturn>" + vDestinationKeyboard
                            : "<Keyboard>" + vDestinationKeyboard;
                    }
                    if (!String.IsNullOrWhiteSpace(vStep.Text))
                    {
                        vStepList += "<Text>" + vStep.Text;
                    }
                    if (!String.IsNullOrWhiteSpace(vStep.Wait))
                    {
                        vStepList += "<Wait>" + vStep.Wait;
                    }
                    //Plugin commands are written here, but will be 
                    //ignored until a new plugin command handler is written
                    if (!String.IsNullOrWhiteSpace(vStep.Plugin) && !String.IsNullOrWhiteSpace(vStep.Method))
                    {
                        vStepList += "<Argment>";
                        foreach (var vArg in vStep.Argument)
                        {
                            if (!String.IsNullOrWhiteSpace(vArg.Name))
                            {
                                vStepList += "<Name>" + vArg.Name;
                            }
                            if (!String.IsNullOrWhiteSpace(vArg.Value))
                            {
                                vStepList += "<Value>" + vArg.Value;
                            }
                        }
                        vStepList += "</Argment>";
                    }
                }

                newKey.Value = new KeyValue(FunctionKeys.StepList, vStepList);
            }
            else
            {
                Log.ErrorFormat("No value found in dynamic key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        void AddOutputKey(XmlOutputKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);

            if (xmlKey.Output != null)
            {
                newKey.Value = new KeyValue(xmlKey.Output);
            }
            else
            {
                Log.ErrorFormat("No value found in output key with row {0}, column {1}", xmlKey.Row, xmlKey.Col);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width, xmlKey.Output);
        }

        void AddPluginKey(XmlPluginKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);

            if (xmlKey.Plugin != null && xmlKey.Method != null)
            {
                // FIXME: Saving the XML of the xmlKey itself probably is not the best option. It is done this way to avoid messing with
                // other pieces of code deep within OptiKey.
                XmlSerializer xmlSer = new XmlSerializer(typeof(XmlPluginKey));
                using (var sww = new StringWriter())
                {
                    XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented };
                    xmlSer.Serialize(writer, xmlKey);
                    newKey.Value = new KeyValue(FunctionKeys.Plugin, sww.ToString());
                }
            }
            else
            {
                Log.ErrorFormat("Incomplete plugin key configuration in key {0}", xmlKey.Label ?? xmlKey.Symbol);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        void AddTextKey(XmlTextKey xmlKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyWithBasicProps(xmlKey, minKeyWidth, minKeyHeight);

            if (xmlKey.Text != null)
            {
                newKey.Value = new KeyValue(xmlKey.Text);
            }
            else
            {
                Log.ErrorFormat("No value found in text key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        private void SetupStyle()
        {
            // Get border and background values, if specified, to override
            if (keyboard.BorderThickness.HasValue)
            {
                Log.InfoFormat("Setting border thickness for custom keyboard: {0}", keyboard.BorderThickness.Value);
                this.BorderThickness = keyboard.BorderThickness.Value;
            }
            if (!string.IsNullOrEmpty(keyboard.BorderColor)
                && (Regex.IsMatch(keyboard.BorderColor, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$")
                    || System.Drawing.Color.FromName(keyboard.BorderColor).IsKnownColor))
            {
                Log.InfoFormat("Setting border color for custom keyboard: {0}", keyboard.BorderColor);
                this.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(keyboard.BorderColor);
            }
            if (!string.IsNullOrEmpty(keyboard.BackgroundColor)
                &&  (Regex.IsMatch(keyboard.BackgroundColor, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$")
                    || System.Drawing.Color.FromName(keyboard.BackgroundColor).IsKnownColor))
            {
                Log.InfoFormat("Setting background color for custom keyboard: {0}", keyboard.BackgroundColor);
                this.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(keyboard.BackgroundColor);
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

            if (keyboard != null && keyboard.ShowOutputPanel)
            {
                // make sure top controls and main grid are scaled appropriately
                TopGrid.RowDefinitions[1].Height = new GridLength(nRows, GridUnitType.Star);
            }
            else
            {
                // hide the output control
                TopGrid.RowDefinitions[0].Height = new GridLength(0);
                OutputPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddColsToGrid(int nCols)
        {
            for (int i = 0; i < nCols; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void PlaceKeyInPosition(Key key, int row, int col, int rowspan = 1, int colspan = 1, string output ="")
        {
            if (output == "Scratchpad")
            {
                var outputControl = new XmlScratchpad();
                MainGrid.Children.Add(outputControl);
                Grid.SetColumn(outputControl, col);
                Grid.SetRow(outputControl, row);
                Grid.SetColumnSpan(outputControl, colspan);
                Grid.SetRowSpan(outputControl, rowspan);
            }
            //else if (output=="Suggestion")
            //{
            //    var outputControl = new XmlSuggestionRow();
            //    MainGrid.Children.Add(outputControl);
            //    Grid.SetColumn(outputControl, col);
            //    Grid.SetRow(outputControl, row);
            //    Grid.SetColumnSpan(outputControl, colspan);
            //    Grid.SetRowSpan(outputControl, rowspan);
            //}
            else
            {
                MainGrid.Children.Add(key);
                Grid.SetColumn(key, col);
                Grid.SetRow(key, row);
                Grid.SetColumnSpan(key, colspan);
                Grid.SetRowSpan(key, rowspan);
            }
        }

        public static string StringWithValidNewlines(string s)
        {
            if (s.Contains("\\r\\n"))
                s = s.Replace("\\r\\n", Environment.NewLine);

            if (s.Contains("\\n"))
                s = s.Replace("\\n", Environment.NewLine);

            return s;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            ShiftAware = keyboard != null && keyboard.IsShiftAware;
        }
    }
}