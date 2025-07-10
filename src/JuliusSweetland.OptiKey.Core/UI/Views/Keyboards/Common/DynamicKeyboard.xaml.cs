// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
using System.Collections.Generic;
using JuliusSweetland.OptiKey.UI.Windows;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ValueConverters;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for DynamicKeyboard.xaml
    /// </summary>
    public partial class DynamicKeyboard : KeyboardView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindow mainWindow;
        private readonly string inputFilename;
        private readonly XmlKeyboard keyboard;
        private readonly IList<Tuple<KeyValue, KeyValue>> keyFamily;
        private readonly IDictionary<string, List<KeyValue>> keyValueByGroup;
        private readonly IDictionary<KeyValue, TimeSpanOverrides> overrideTimesByKey;
        private readonly IWindowManipulationService windowManipulationService;

        public DynamicKeyboard(
            MainWindow parentWindow, 
            string inputFile, 
            IList<Tuple<KeyValue, KeyValue>> keyFamily, 
            IDictionary<string, List<KeyValue>> keyValueByGroup, 
            IDictionary<KeyValue, TimeSpanOverrides> overrideTimesByKey,
            IWindowManipulationService windowManipulationService)
        {
            InitializeComponent();
            this.mainWindow = parentWindow;
            inputFilename = inputFile;
            this.keyFamily = keyFamily;
            this.keyValueByGroup = keyValueByGroup;
            this.overrideTimesByKey = overrideTimesByKey;
            this.windowManipulationService = windowManipulationService;

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

            if (!ValidateKeyboard()) { return; }

            // New logic for content keyboard
            if (keyboard.Content != null)
            {
                SetupGrid(); // Setup all the UI components
                if (!SetupDynamicItems()) { return; }
            }
            // Legacy logic
            else
            {
                if (!ValidateKeys()) { return; }
                SetupGrid(); // Setup all the UI components 
                SetupKeys();
            }

            SetupStyle(); // Set the override border and background colors 
        }

        private bool ValidateKeyboard()
        {
            string errorMessage = null;
            if (!string.IsNullOrWhiteSpace(keyboard.WindowState) && Enum.TryParse(keyboard.WindowState, out WindowStates validWindowState)
                                                                 && validWindowState != WindowStates.Docked && validWindowState != WindowStates.Floating && validWindowState != WindowStates.Maximised)
                errorMessage = "WindowState not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.Position) && !Enum.TryParse<MoveToDirections>(keyboard.Position, out _))
                errorMessage = "Position not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.DockSize) && !Enum.TryParse<DockSizes>(keyboard.DockSize, out var validDockSize))
                errorMessage = "DockSize not valid";
            else if (!string.IsNullOrWhiteSpace(keyboard.Width) &&
                !(double.TryParse(keyboard.Width.Replace("%", ""), out var validNumber) && validNumber >= -9999 && validNumber <= 9999))
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

            else if (keyboard.Grid == null)
                errorMessage = "No grid definition found";
            else if (keyboard.Grid.Rows < 1 || keyboard.Grid.Cols < 1)
                errorMessage = "Grid size is " + keyboard.Grid.Rows + " rows and " + keyboard.Grid.Cols + " columns";
            else if ((keyboard.Keys == null || keyboard.Keys.Count == 0) && keyboard.Content == null)
                errorMessage = "No key or content definitions found";

            if (errorMessage != null)
            {
                SetupErrorLayout("Invalid keyboard file", errorMessage);
                return false;
            }

            // If the keyboard overrides any size/position values, tell the windowsManipulationService that it shouldn't be persisting state changes
            if (!string.IsNullOrWhiteSpace(keyboard.WindowState)
                || !string.IsNullOrWhiteSpace(keyboard.Position)
                || !string.IsNullOrWhiteSpace(keyboard.DockSize)
                || !string.IsNullOrWhiteSpace(keyboard.Width)
                || !string.IsNullOrWhiteSpace(keyboard.Height)
                || !string.IsNullOrWhiteSpace(keyboard.HorizontalOffset)
                || !string.IsNullOrWhiteSpace(keyboard.VerticalOffset))
            {
                Log.InfoFormat("Overriding size and position for dynamic keyboard");
                windowManipulationService.OverridePersistedState(keyboard.PersistNewState, keyboard.WindowState,
                    keyboard.Position, keyboard.DockSize, keyboard.Width, keyboard.Height, keyboard.HorizontalOffset,
                    keyboard.VerticalOffset);
            }

            return true;
        }

        private bool ValidateKeys()
        {
            var allKeys = keyboard.Keys.ActionKeys.Cast<IXmlKey>()
                .Concat(keyboard.Keys.ChangeKeyboardKeys)
                .Concat(keyboard.Keys.DynamicKeys)
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
                var keyStrings = keys.Select(GetKeyString).Aggregate((seq, next) => $"{seq}, {next}");
                return $"{keyStrings} ({keys.First().Row}, {keys.First().Col})";
            }).Aggregate((msg, key) => $"{msg}, {key}");

            SetupErrorLayout("Duplicate row/column values for keys", errorMsg);
            return false;
        }

        private string GetKeyString(IXmlKey xmlKey)
        {
            if (xmlKey is XmlTextKey textKey)
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
                string xmlKeyLabel = xmlKey.Label;
                string oldValue;
                string newValue;
                while (xmlKeyLabel.Contains("{Resource:"))
                {
                    oldValue = xmlKeyLabel.Substring(xmlKeyLabel.IndexOf("{Resource:"), xmlKeyLabel.IndexOf("}", xmlKeyLabel.IndexOf("{Resource:")) - xmlKeyLabel.IndexOf("{Resource:") + 1);
                    newValue = Properties.Resources.ResourceManager.GetString(oldValue.Substring(10, oldValue.Length - 11).Trim());
                    xmlKeyLabel = xmlKeyLabel.Replace(oldValue, newValue);
                }
                while (xmlKeyLabel.Contains("{Setting:"))
                {
                    oldValue = xmlKeyLabel.Substring(xmlKeyLabel.IndexOf("{Setting:"), xmlKeyLabel.IndexOf("}", xmlKeyLabel.IndexOf("{Setting:")) - xmlKeyLabel.IndexOf("{Setting:") + 1);
                    newValue = Properties.Settings.Default[oldValue.Substring(9, oldValue.Length - 10).Trim()].ToString();
                    xmlKeyLabel = xmlKeyLabel.Replace(oldValue, newValue);
                }

                newKey.Text = xmlKeyLabel.ToStringWithValidNewlines();
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

                    newKey.SharedSizeGroup = text != null && text.Length > 5
                        ? "KeyWithLongText" : text != null && text.Length > 1
                        ? "KeyWithShortText" : "KeyWithSingleLetter";
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
            info = info.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];

            // Wrap to (approx) three lines
            var len = info.Length;
            var maxLineLength = len / 3.5;
            Log.Info(maxLineLength);
            char[] space = { ' ' };

            var charCount = 0;
            var allLines = info.Split(space)
                .GroupBy(w => (int)((charCount += w.Length + 1) / maxLineLength))
                .Select(g => string.Join(" ", g));

            return string.Join(Environment.NewLine, allLines);
        }

        private void SetupErrorLayout(string heading, string content)
        {
            if (MainGrid.Children.Count > 0)
                MainGrid.Children.RemoveRange(0, MainGrid.Children.Count);
            if (MainGrid.ColumnDefinitions.Count > 0)
                MainGrid.ColumnDefinitions.RemoveRange(0, MainGrid.ColumnDefinitions.Count);
            if (MainGrid.RowDefinitions.Count > 0)
                MainGrid.RowDefinitions.RemoveRange(0, MainGrid.RowDefinitions.Count);
            AddRowsToGrid(4);
            AddColsToGrid(4);

            // We hardcode black-on-white text for full visibility

            // Top middle two cells are main error message
            {
                var newKey = new Key {
                    Text = heading,
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };

                PlaceKeyInPosition(newKey, 0, 1, 1, 2);                
            }
            // Middle row is detailed error message
            {
                var newKey = new Key {
                    Text = content,
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };

                PlaceKeyInPosition(newKey, 1, 0, 2, 4);
            }

            // Back key
            {
                var newKey = new Key
                {
                    SymbolGeometry = (Geometry)Application.Current.Resources["BackIcon"],
                    Text = Properties.Resources.BACK,
                    Value = KeyValues.BackFromKeyboardKey,
                    ForegroundColourOverride = Brushes.Black,
                    BackgroundColourOverride = Brushes.White
                };
                PlaceKeyInPosition(newKey, 3, 3);
            }

            // Fill in empty keys
            {
                var newKey = new Key {
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };
                PlaceKeyInPosition(newKey, 0, 0, 1, 1);
            }
            {
                var newKey = new Key { 
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };
                PlaceKeyInPosition(newKey, 0, 3, 1, 1);
            }
            {
                var newKey = new Key {
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };
                PlaceKeyInPosition(newKey, 3, 0, 1, 1);
            }
            {
                var newKey = new Key {
                    DisabledForegroundColourOverride = Brushes.Black,
                    DisabledBackgroundColourOverride = Brushes.White
                };
                PlaceKeyInPosition(newKey, 3, 1, 1, 2);
            }

            // Set as default floating window size, i.e. pretty large
            // This ensures the error message and keys are a reasonable size!            
            windowManipulationService.OverridePersistedState(false, "Floating",
                    "Top", "", "60%", "60%", "0", "0"); // Empty strings will allow defaults to be used instead
            
        }

        private bool SetupDynamicItems()
        {
            var minKeyWidth = keyboard.Content.Items.Select(k => k.Width).Min() > 0 ? keyboard.Content.Items.Select(k => k.Width).Min() : 1;
            var minKeyHeight = keyboard.Content.Items.Select(k => k.Height).Min() > 0 ? keyboard.Content.Items.Select(k => k.Height).Min() : 1;

            //start with a list of all grid cells marked empty
            List<List<int>> openGrid = new List<List<int>>();
            for (int r = 0; r < keyboard.Grid.Rows; r++)
            {
                List<int> gridRow = new List<int>();
                for (int c = 0; c < keyboard.Grid.Cols; c++)
                {
                    gridRow.Add(c);
                }
                openGrid.Add(gridRow);
            }

            //begin section 1: processing items with a reserved row and column
            var itemPosition = keyboard.Content.Items.ToList();
            foreach (XmlDynamicItem dynamicItem in itemPosition.Where(x => x.Row > -1 && x.Col > -1))
            {
                var vIndex = keyboard.Content.Items.IndexOf(dynamicItem);
                var vLabel = " with type of Suggestion";
                if (dynamicItem is XmlDynamicKey dynamicKey)
                {
                    vLabel = (!string.IsNullOrEmpty(dynamicKey.Label)) ? " with label '" + dynamicKey.Label + "'"
                        : (!string.IsNullOrEmpty(dynamicKey.Symbol)) ? " with symbol '" + dynamicKey.Symbol + "'"
                        : " with no label or symbol";
                }
                else if (dynamicItem is XmlDynamicScratchpad)
                {
                    vLabel = " with type of Scratchpad";
                }

                if (dynamicItem.Col + dynamicItem.Width > keyboard.Grid.Cols || dynamicItem.Row + dynamicItem.Height > keyboard.Grid.Rows)
                {
                    SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                        + (vIndex + 1) + " of " + itemPosition.Count + vLabel
                        + " at row " + dynamicItem.Row + " column " + dynamicItem.Col);
                    return false;
                }
                //find space to allocate and remove it from available
                for (int row = dynamicItem.Row; row < (dynamicItem.Row + dynamicItem.Height); row++)
                {
                    for (int col = dynamicItem.Col; col < (dynamicItem.Col + dynamicItem.Width); col++)
                    {
                        if (!openGrid[row].Exists(x => x == col)) //if the column is unavailable
                        {
                            SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                                + (vIndex + 1) + " of " + itemPosition.Count + vLabel
                                + " at row " + dynamicItem.Row + " column " + dynamicItem.Col);
                            return false;
                        }
                        else
                            openGrid[row].Remove(col);
                    }
                }
                SetupDynamicItem(dynamicItem, minKeyWidth, minKeyHeight);
            }
            //end section 1: processing items with a reserved row and column

            //begin section 2: processing items that need a row and/or column assigned
            //the items are processed in the same order they were listed in the xml file
            //if an item has a row or column designation it is treated as an indication to jump forward 
            //to that row or column and mark any/all skipped spaces as empty
            foreach (XmlDynamicItem dynamicItem in itemPosition.Where(x => !(x.Row > -1 && x.Col > -1)))
            {
                var vIndex = itemPosition.IndexOf(dynamicItem);
                var vLabel = " with type of Suggestion";
                if (dynamicItem is XmlDynamicKey dynamicKey)
                {
                    vLabel = (!string.IsNullOrEmpty(dynamicKey.Label)) ? " with label '" + dynamicKey.Label + "'"
                        : (!string.IsNullOrEmpty(dynamicKey.Symbol)) ? " with symbol '" + dynamicKey.Symbol + "'"
                        : " with no label or symbol";
                }
                else if (dynamicItem is XmlDynamicScratchpad)
                {
                    vLabel = " with type of Scratchpad";
                }

                bool itemPositionConfirmed = false;
                while (!itemPositionConfirmed)
                {
                    var vItemColumn = 0;
                    var vRowsConfirmed = 0;

                    //set start row to first row with enough available width for the item
                    var startRow = dynamicItem.Row > -1 ? (openGrid[dynamicItem.Row].Count >= dynamicItem.Width ? dynamicItem.Row : -1)
                        : openGrid.FindIndex(x => (x.Count >= dynamicItem.Width));
                    //if a start row with enough empty space is not found then return an error
                    if (startRow < 0)
                    {
                        SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                            + (vIndex + 1) + " of " + itemPosition.Count + vLabel + " having width "
                            + dynamicItem.Width + " and height " + dynamicItem.Height);
                        return false;
                    }
                    //block all preceding columns in all preceding rows
                    for (int row = 0; row < startRow; row++)
                    {
                        if (openGrid[row].Any())
                            openGrid[row].Clear();
                    }
                    for (int row = startRow; row < keyboard.Grid.Rows; row++)
                    {
                        if (dynamicItem.Col > -1)
                        {
                            //if column exists then block all preceding columns in the row
                            if (openGrid[row].Exists(x => x == dynamicItem.Col))
                                openGrid[row].RemoveAll(x => x < dynamicItem.Col);
                            //else block the whole row and check another 
                            else
                            {
                                openGrid[row].Clear();
                                break;
                            }
                        }
                        //if height > 1 and we are searching subsequent rows then we need to start at the confirmed start column
                        var vColsConfirmed = 0;
                        var startColumn = (vRowsConfirmed > 0) ? vItemColumn : (dynamicItem.Col > -1) ? dynamicItem.Col : openGrid[row].First();
                        while (openGrid[row].Any())
                        {
                            //if the next open space is adjacent then increment columns confirmed
                            if (openGrid[row].First() == startColumn + vColsConfirmed)
                            {
                                vColsConfirmed++;
                                openGrid[row].RemoveAt(0);
                                //stop searching if we meet the width requirement
                                if (vColsConfirmed == dynamicItem.Width)
                                    break;
                            }
                            //else if this row does not have enough additional space then block what remains and break
                            else if (openGrid[row].Count < dynamicItem.Width || dynamicItem.Col > -1)
                            {
                                openGrid[row].Clear();
                                break;
                            }
                            //else if the next open space is not adjacent then we need to reset
                            else
                            {
                                vColsConfirmed = 0;
                                startColumn = openGrid[row].First();
                            }
                        }

                        if (vColsConfirmed == dynamicItem.Width)
                        {
                            vItemColumn = startColumn;
                            vRowsConfirmed++;

                            if (vRowsConfirmed == dynamicItem.Height)
                            {
                                dynamicItem.Col = vItemColumn;
                                dynamicItem.Row = 1 + row - dynamicItem.Height;
                                itemPositionConfirmed = true;
                                break;
                            }
                        }
                    } //loop back to process the next row
                }

                SetupDynamicItem(dynamicItem, minKeyWidth, minKeyHeight);
            }
            //end section 2: processing items that need a row and/or column assigned

            return true;
        }

        private void SetupDynamicItem(XmlDynamicItem dynamicItem, int minKeyWidth, int minKeyHeight)
        {
            List<XmlKeyGroup> keyGroupList = new List<XmlKeyGroup>();
            keyGroupList.AddRange(keyboard.KeyGroups.Where(x => x.Name.ToUpper() == "ALL" || dynamicItem.KeyGroups.Exists(y => y.Value == x.Name)));

            if (dynamicItem is XmlDynamicKey xmlDynamicKey)
            {
                AddDynamicKey(xmlDynamicKey, minKeyWidth, minKeyHeight);
            }
            else if (dynamicItem is XmlDynamicScratchpad)
            {
                var scratchpad = new XmlScratchpad();
                MainGrid.Children.Add(scratchpad);
                Grid.SetColumn(scratchpad, dynamicItem.Col);
                Grid.SetRow(scratchpad, dynamicItem.Row);
                Grid.SetColumnSpan(scratchpad, dynamicItem.Width);
                Grid.SetRowSpan(scratchpad, dynamicItem.Height);

                if (ValidColor(dynamicItem.BackgroundColor, out var colorBrush))
                    scratchpad.Scratchpad.BackgroundColourOverride = colorBrush;
                else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BackgroundColor, out colorBrush)))
                    scratchpad.Scratchpad.BackgroundColourOverride = colorBrush;

                if (ValidColor(dynamicItem.ForegroundColor, out colorBrush))
                    scratchpad.Scratchpad.ForegroundColourOverride = colorBrush;
                else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.ForegroundColor, out colorBrush)))
                    scratchpad.Scratchpad.ForegroundColourOverride = colorBrush;

                if (!string.IsNullOrEmpty(dynamicItem.Opacity) && double.TryParse(dynamicItem.Opacity, out var opacity))
                    scratchpad.Scratchpad.OpacityOverride = opacity;
            }
            else
            {
                if (dynamicItem is XmlDynamicSuggestionGrid grid)
                {
                    var suggestionGrid = new XmlSuggestionGrid(grid.NumRows, grid.NumCols, grid.RightToLeft, grid.BottomToTop);
                    suggestionGrid.DataContext = this.DataContext;
                    MainGrid.Children.Add(suggestionGrid);
                    Grid.SetColumn(suggestionGrid, dynamicItem.Col);
                    Grid.SetRow(suggestionGrid, dynamicItem.Row);
                    Grid.SetColumnSpan(suggestionGrid, dynamicItem.Width);
                    Grid.SetRowSpan(suggestionGrid, dynamicItem.Height);

                    if (!string.IsNullOrEmpty(dynamicItem.Opacity) && double.TryParse(dynamicItem.Opacity, out var opacity))
                        suggestionGrid.Opacity = opacity;

                    // Apply colors to all keys within the grid
                    foreach (var child in suggestionGrid.MainGrid.Children.OfType<Key>())
                    {
                        if (ValidColor(dynamicItem.BackgroundColor, out var colorBrush))
                            child.BackgroundColourOverride = colorBrush;
                        else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BackgroundColor, out colorBrush)))
                            child.BackgroundColourOverride = colorBrush;

                        if (ValidColor(dynamicItem.ForegroundColor, out colorBrush))
                            child.ForegroundColourOverride = colorBrush;
                        else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.ForegroundColor, out colorBrush)))
                            child.ForegroundColourOverride = colorBrush;
                    }
                }

                if (dynamicItem is XmlDynamicSuggestionRow)
                {
                    var suggestionRow = new XmlSuggestionRow();
                    MainGrid.Children.Add(suggestionRow);
                    Grid.SetColumn(suggestionRow, dynamicItem.Col);
                    Grid.SetRow(suggestionRow, dynamicItem.Row);
                    Grid.SetColumnSpan(suggestionRow, dynamicItem.Width);
                    Grid.SetRowSpan(suggestionRow, dynamicItem.Height);

                    if (!string.IsNullOrEmpty(dynamicItem.Opacity) && double.TryParse(dynamicItem.Opacity, out var opacity))
                        suggestionRow.OpacityOverride = opacity;

                    // Apply colors to all keys within the row
                    foreach (var child in suggestionRow.Content is Grid _grid ? _grid.Children.OfType<Key>() : Enumerable.Empty<Key>())
                    {
                        if (ValidColor(dynamicItem.BackgroundColor, out var colorBrush))
                            child.BackgroundColourOverride = colorBrush;
                        else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BackgroundColor, out colorBrush)))
                            child.BackgroundColourOverride = colorBrush;

                        if (ValidColor(dynamicItem.ForegroundColor, out colorBrush))
                            child.ForegroundColourOverride = colorBrush;
                        else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.ForegroundColor, out colorBrush)))
                            child.ForegroundColourOverride = colorBrush;
                    }
                }
                else if (dynamicItem is XmlDynamicSuggestionCol)
                {
                    var suggestionCol = new XmlSuggestionCol();
                    MainGrid.Children.Add(suggestionCol);
                    Grid.SetColumn(suggestionCol, dynamicItem.Col);
                    Grid.SetRow(suggestionCol, dynamicItem.Row);
                    Grid.SetColumnSpan(suggestionCol, dynamicItem.Width);
                    Grid.SetRowSpan(suggestionCol, dynamicItem.Height);

                    if (ValidColor(dynamicItem.BackgroundColor, out var colorBrush))
                    {
                        suggestionCol.Background = colorBrush;
                        suggestionCol.DisabledBackgroundColourOverride = colorBrush;
                    }
                    else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BackgroundColor, out colorBrush)))
                    {
                        suggestionCol.Background = colorBrush;
                        suggestionCol.DisabledBackgroundColourOverride = colorBrush;
                    }

                    if (ValidColor(dynamicItem.ForegroundColor, out colorBrush))
                        suggestionCol.Foreground = colorBrush;
                    else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.ForegroundColor, out colorBrush)))
                        suggestionCol.Foreground = colorBrush;

                    if (!string.IsNullOrEmpty(dynamicItem.Opacity) && double.TryParse(dynamicItem.Opacity, out var suggestionColOpacity))
                        suggestionCol.Opacity = suggestionColOpacity;
                }
            }
        }

        private void AddDynamicKey(XmlDynamicKey xmlDynamicKey, int minKeyWidth, int minKeyHeight)
        {
            if (xmlDynamicKey.Commands.Any())
            {
                var addCommandList = AddCommandList(xmlDynamicKey, minKeyWidth, minKeyHeight);
                if (addCommandList != null && addCommandList.Any())
                {
                    var xmlKeyValue = new KeyValue($"R{xmlDynamicKey.Row}-C{xmlDynamicKey.Col}")
                    {
                        Commands = addCommandList
                    };
                    CreateDynamicKey(xmlDynamicKey, xmlKeyValue, minKeyWidth, minKeyHeight);
                }
            }
            //place a key that performs no action
            else
                CreateDynamicKey(xmlDynamicKey, null, minKeyWidth, minKeyHeight);
        }

        private List<KeyCommand> AddCommandList(XmlDynamicKey xmlDynamicKey, int minKeyWidth, int minKeyHeight)
        {
            var xmlKeyValue = new KeyValue($"R{xmlDynamicKey.Row}-C{xmlDynamicKey.Col}");
            var commandList = new List<KeyCommand>();
            if (xmlDynamicKey.Commands.Any())
            {
                var rootDir = Path.GetDirectoryName(inputFilename);
                foreach (XmlDynamicKey dynamicKey in xmlDynamicKey.Commands)
                {
                    KeyValue commandKeyValue;
                    if (dynamicKey is DynamicAction dynamicAction)
                    {
                        if (!Enum.TryParse(dynamicAction.Value, out FunctionKeys actionEnum))
                            Log.ErrorFormat("Could not parse {0} as function key", dynamicAction.Value);
                        else
                        {
                            commandKeyValue = new KeyValue(actionEnum);
                            if (xmlDynamicKey.Commands.Count == 1 && KeyValues.KeysWhichCanBeLockedDown.Contains(commandKeyValue))
                            {
                                CreateDynamicKey(xmlDynamicKey, commandKeyValue, minKeyWidth, minKeyHeight);
                                return null;
                            }
                            else
                                commandList.Add(new KeyCommand(KeyCommands.Function, dynamicAction.Value));

                            if (KeyValues.KeysWhichCanBeLockedDown.Contains(commandKeyValue) 
                                && !keyFamily.Contains(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue)))
                            {
                                keyFamily.Add(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue));
                            }
                        }
                    }
                    else if (dynamicKey is DynamicLink dynamicLink)
                    {
                        if (string.IsNullOrEmpty(dynamicLink.Value))
                            Log.ErrorFormat("Destination Keyboard not found for {0} ", dynamicLink.Label);
                        else
                        {
                            var kb_link = Enum.TryParse(dynamicLink.Value, out Enums.Keyboards keyboardEnum)
                                ? dynamicLink.Value : Path.Combine(rootDir, dynamicLink.Value);

                            commandList.Add(new KeyCommand() { Name = KeyCommands.ChangeKeyboard, Value = kb_link, BackAction = !dynamicLink.BackReturnsHere });
                        }
                    }
                    else if (dynamicKey is DynamicKeyDown dynamicKeyDown)
                    {
                        if (string.IsNullOrEmpty(dynamicKeyDown.Value))
                            Log.ErrorFormat("KeyDown text not found for {0} ", dynamicKeyDown.Label);
                        else
                        {
                            commandKeyValue = new KeyValue(dynamicKeyDown.Value);
                            commandList.Add(new KeyCommand(KeyCommands.KeyDown, dynamicKeyDown.Value));
                            if (!keyFamily.Contains(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue)))
                                keyFamily.Add(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue));
                        }
                    }
                    else if (dynamicKey is DynamicKeyToggle dynamicKeyToggle)
                    {
                        if (string.IsNullOrEmpty(dynamicKeyToggle.Value))
                            Log.ErrorFormat("KeyToggle text not found for {0} ", dynamicKeyToggle.Label);
                        else
                        {
                            commandKeyValue = new KeyValue(dynamicKeyToggle.Value);
                            commandList.Add(new KeyCommand(KeyCommands.KeyToggle, dynamicKeyToggle.Value));
                            if (!keyFamily.Contains(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue)))
                                keyFamily.Add(new Tuple<KeyValue, KeyValue>(xmlKeyValue, commandKeyValue));
                        }
                    }
                    else if (dynamicKey is DynamicKeyUp dynamicKeyUp)
                    {
                        if (string.IsNullOrEmpty(dynamicKeyUp.Value))
                            Log.ErrorFormat("KeyUp text not found for {0} ", dynamicKeyUp.Label);
                        else
                            commandList.Add(new KeyCommand(KeyCommands.KeyUp, dynamicKeyUp.Value));
                    }
                    else if (dynamicKey is DynamicMove dynamicBounds)
                    {
                        commandList.Add(new KeyCommand() { Name = KeyCommands.MoveWindow, Value = dynamicBounds.Value } );
                    }
                    else if (dynamicKey is DynamicText dynamicText)
                    {
                        if (string.IsNullOrEmpty(dynamicText.Value))
                            Log.ErrorFormat("Text not found for {0} ", dynamicText.Label);
                        else
                            commandList.Add(new KeyCommand(KeyCommands.Text, dynamicText.Value));
                    }
                    else if (dynamicKey is DynamicWait dynamicWait)
                    {
                        if (!int.TryParse(dynamicWait.Value, out _))
                            Log.ErrorFormat("Could not parse wait {0} as int value", dynamicWait.Label);
                        else
                            commandList.Add(new KeyCommand() { Name = KeyCommands.Wait, Value = dynamicWait.Value } );
                    }
                    else if (dynamicKey is DynamicPlugin dynamicPlugin)
                    {
                        if (string.IsNullOrWhiteSpace(dynamicPlugin.Name))
                            Log.ErrorFormat("Plugin not found for {0} ", dynamicPlugin.Label);
                        else if (string.IsNullOrWhiteSpace(dynamicPlugin.Method))
                            Log.ErrorFormat("Method not found for {0} ", dynamicPlugin.Label);
                        else
                            commandList.Add(new KeyCommand() { Name = KeyCommands.Plugin, Value = dynamicPlugin.Name,
                                Method = dynamicPlugin.Method, Argument = dynamicPlugin.Argument } );
                    }
                    else if (dynamicKey is DynamicLoop dynamicLoop)
                    {
                        var vReturn = AddCommandList(dynamicLoop, minKeyWidth, minKeyHeight);
                        if (vReturn != null && vReturn.Any())
                            commandList.Add(new KeyCommand() { Name = KeyCommands.Loop, Value = dynamicLoop.Count.ToString(), LoopCommands = vReturn } );
                        else
                            return null;
                    }
                }
            }
            else
            {
                Log.ErrorFormat("No value found in dynamic key with label {0}", xmlDynamicKey.Label);
            }
            return commandList;
        }

        private void CreateDynamicKey(XmlDynamicKey xmlKey, KeyValue xmlKeyValue, int minKeyWidth, int minKeyHeight)
        {
            // Add the core properties from XML to a new key
            var newKey = new Key { Value = xmlKeyValue };
            
            //add this item's KeyValue to the 'ALL' KeyGroup list
            if (!keyValueByGroup.ContainsKey("ALL"))
                keyValueByGroup.Add("ALL", new List<KeyValue> { xmlKeyValue });
            else if (!keyValueByGroup["ALL"].Contains(xmlKeyValue))
                keyValueByGroup["ALL"].Add(xmlKeyValue);

            //add this item's KeyValue to each KeyGroup referenced in its definition
            foreach (KeyGroup vKeyGroup in xmlKey.KeyGroups)
            {
                if (!keyValueByGroup.ContainsKey(vKeyGroup.Value.ToUpper()))
                    keyValueByGroup.Add(vKeyGroup.Value.ToUpper(), new List<KeyValue> { xmlKeyValue });
                else if (!keyValueByGroup[vKeyGroup.Value.ToUpper()].Contains(xmlKeyValue))
                    keyValueByGroup[vKeyGroup.Value.ToUpper()].Add(xmlKeyValue);
            }

            if (xmlKey.Label != null)
            {
                string label = xmlKey.Label;
                string vText;
                string vLookup;
                while (label.Contains("{Resource:"))
                {
                    vText = label.Substring(label.IndexOf("{Resource:"), label.IndexOf("}", label.IndexOf("{Resource:")) - label.IndexOf("{Resource:") + 1);
                    vLookup = Properties.Resources.ResourceManager.GetString(vText.Substring(10, vText.Length - 11).Trim());
                    label = label.Replace(vText, vLookup);
                }
                while (label.Contains("{Setting:"))
                {
                    vText = label.Substring(label.IndexOf("{Setting:"), label.IndexOf("}", label.IndexOf("{Setting:")) - label.IndexOf("{Setting:") + 1);
                    vLookup = Properties.Settings.Default[vText.Substring(9, vText.Length - 10).Trim()].ToString();
                    label = label.Replace(vText, vLookup);
                }

                newKey.Text = label.ToStringWithValidNewlines();
            }

            if (xmlKey.Label != null && xmlKey.ShiftDownLabel != null)
            {
                newKey.ShiftUpText = xmlKey.Label.ToStringWithValidNewlines();
                newKey.ShiftDownText = xmlKey.ShiftDownLabel.ToStringWithValidNewlines();
            }

            if (xmlKey.Symbol != null)
            {
                Geometry geom = (Geometry)this.Resources[xmlKey.Symbol];
                if (geom != null)
                    newKey.SymbolGeometry = geom;
                else
                    Log.ErrorFormat("Could not parse {0} as symbol geometry", xmlKey.Symbol);
            }

            // Add same symbol margin to all keys
            if (keyboard.SymbolMargin.HasValue)
                newKey.SymbolMargin = keyboard.SymbolMargin.Value;

            //Create a list and add all the keyboard's attribute KeyGroup that are referenced by this key
            List<XmlKeyGroup> keyGroupList = new List<XmlKeyGroup>();
            keyGroupList.AddRange(keyboard.KeyGroups.Where(x => x.Name.ToUpper() == "ALL" || xmlKey.KeyGroups.Exists(y => y.Value == x.Name)));

            // Set shared size group
            if (!string.IsNullOrEmpty(xmlKey.SharedSizeGroup))
                newKey.SharedSizeGroup = xmlKey.SharedSizeGroup;
            else if (keyGroupList != null && keyGroupList.Exists(x => x.SharedSizeGroup != null))
                newKey.SharedSizeGroup = keyGroupList.Find(x => x.SharedSizeGroup != null).SharedSizeGroup;
            else
            {
                bool hasSymbol = newKey.SymbolGeometry != null;
                bool hasString = xmlKey.Label != null || xmlKey.ShiftDownLabel != null;
                if (hasSymbol && hasString)
                    newKey.SharedSizeGroup = "KeyWithSymbolAndText";
                else if (hasSymbol)
                    newKey.SharedSizeGroup = "KeyWithSymbol";
                else if (hasString)
                {
                    var text = newKey.Text != null ? newKey.Text.Compose() : newKey.ShiftDownText?.Compose();

                    //Strip out circle character used to show diacritic marks
                    text = text?.Replace("\x25CC", string.Empty);

                    newKey.SharedSizeGroup = text != null && text.Length > 5
                        ? "KeyWithLongText" : text != null && text.Length > 1
                        ? "KeyWithShortText" : "KeyWithSingleLetter";
                }
            }

            //Auto set width span and height span
            if (xmlKey.AutoScaleToOneKeyWidth.HasValue && xmlKey.AutoScaleToOneKeyWidth.Value)
                newKey.WidthSpan = (double)xmlKey.Width / (double)minKeyWidth;
            else if (!xmlKey.AutoScaleToOneKeyWidth.HasValue
                && (keyGroupList == null || keyGroupList.Exists(x => x.AutoScaleToOneKeyWidth.HasValue
                    && !x.AutoScaleToOneKeyWidth.Value)))
                newKey.WidthSpan = (double)xmlKey.Width / (double)minKeyWidth;
            if (xmlKey.AutoScaleToOneKeyHeight.HasValue && xmlKey.AutoScaleToOneKeyHeight.Value)
                newKey.WidthSpan = (double)xmlKey.Width / (double)minKeyWidth;
            else if (!xmlKey.AutoScaleToOneKeyHeight.HasValue
                && (keyGroupList == null || keyGroupList.Exists(x => x.AutoScaleToOneKeyHeight.HasValue
                    && !x.AutoScaleToOneKeyHeight.Value)))
                newKey.WidthSpan = (double)xmlKey.Width / (double)minKeyWidth;


            if (xmlKey.UsePersianCompatibilityFont)
                newKey.UsePersianCompatibilityFont = true;
            else if (keyGroupList != null && keyGroupList.Exists(x => x.UsePersianCompatibilityFont))
                newKey.UsePersianCompatibilityFont = true;

            if (xmlKey.UseUnicodeCompatibilityFont)
                newKey.UseUnicodeCompatibilityFont = true;
            else if (keyGroupList != null && keyGroupList.Exists(x => x.UseUnicodeCompatibilityFont))
                newKey.UseUnicodeCompatibilityFont = true;

            if (xmlKey.UseUrduCompatibilityFont)
                newKey.UseUrduCompatibilityFont = true;
            else if (keyGroupList != null && keyGroupList.Exists(x => x.UseUrduCompatibilityFont))
                newKey.UseUrduCompatibilityFont = true;

            if (ValidColor(xmlKey.ForegroundColor, out var colorBrush))
                newKey.ForegroundColourOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.ForegroundColor, out colorBrush)))
                newKey.ForegroundColourOverride = colorBrush;

            if (ValidColor(xmlKey.KeyDisabledForeground, out colorBrush))
                newKey.DisabledForegroundColourOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.KeyDisabledForeground, out colorBrush)))
                newKey.DisabledForegroundColourOverride = colorBrush;
            else if (newKey.ForegroundColourOverride != null)
                newKey.DisabledForegroundColourOverride = new SolidColorBrush(HlsColor.Fade(((SolidColorBrush)newKey.ForegroundColourOverride).Color, .15)); 

            if (ValidColor(xmlKey.KeyDownForeground, out colorBrush))
                newKey.KeyDownForegroundOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.KeyDownForeground, out colorBrush)))
                newKey.KeyDownForegroundOverride = colorBrush;
            else if (newKey.ForegroundColourOverride != null)
                newKey.KeyDownForegroundOverride = new SolidColorBrush(HlsColor.Fade(((SolidColorBrush)newKey.ForegroundColourOverride).Color, .15));

            if (ValidColor(xmlKey.BackgroundColor, out colorBrush))
                newKey.BackgroundColourOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BackgroundColor, out colorBrush)))
                newKey.BackgroundColourOverride = colorBrush;

            if (ValidColor(xmlKey.KeyDisabledBackground, out colorBrush))
                newKey.DisabledBackgroundColourOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.KeyDisabledBackground, out colorBrush)))
                newKey.DisabledBackgroundColourOverride = colorBrush;
            else if (newKey.BackgroundColourOverride != null)
                newKey.DisabledBackgroundColourOverride = new SolidColorBrush(HlsColor.Fade(((SolidColorBrush)newKey.BackgroundColourOverride).Color, .15));

            if (ValidColor(xmlKey.KeyDownBackground, out colorBrush))
                newKey.KeyDownBackgroundOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.KeyDownBackground, out colorBrush)))
                newKey.KeyDownBackgroundOverride = colorBrush;
            else if (newKey.BackgroundColourOverride != null)
                newKey.KeyDownBackgroundOverride = new SolidColorBrush(HlsColor.Fade(((SolidColorBrush)newKey.BackgroundColourOverride).Color, .15));

            if (ValidColor(xmlKey.BorderColor, out colorBrush))
                newKey.BorderColourOverride = colorBrush;
            else if (keyGroupList != null && keyGroupList.Exists(x => ValidColor(x.BorderColor, out colorBrush)))
                newKey.BorderColourOverride = colorBrush;

            int borderThickness = 1;
            if (!string.IsNullOrEmpty(xmlKey.BorderThickness) && int.TryParse(xmlKey.BorderThickness, out borderThickness))
                newKey.BorderThicknessOverride = borderThickness;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.BorderThickness) && int.TryParse(x.BorderThickness, out borderThickness)))
                newKey.BorderThicknessOverride = borderThickness;

            int cornerRadius = 0;
            if (!string.IsNullOrEmpty(xmlKey.CornerRadius) && int.TryParse(xmlKey.CornerRadius, out cornerRadius))
                newKey.CornerRadiusOverride = cornerRadius;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.CornerRadius) && int.TryParse(x.CornerRadius, out cornerRadius)))
                newKey.CornerRadiusOverride = cornerRadius;

            int margin = 0;
            if (!string.IsNullOrEmpty(xmlKey.Margin) && int.TryParse(xmlKey.Margin, out margin))
                newKey.MarginOverride = margin;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.Margin) && int.TryParse(x.Margin, out margin)))
                newKey.MarginOverride = margin;

            double opacity = 1;
            if (!string.IsNullOrEmpty(xmlKey.Opacity) && double.TryParse(xmlKey.Opacity, out opacity))
                newKey.OpacityOverride = opacity;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.Opacity) && double.TryParse(x.Opacity, out opacity)))
                newKey.OpacityOverride = opacity;

            if (!string.IsNullOrEmpty(xmlKey.KeyDisabledOpacity) && double.TryParse(xmlKey.KeyDisabledOpacity, out opacity))
                newKey.DisabledBackgroundOpacity = opacity;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.KeyDisabledOpacity) && double.TryParse(x.KeyDisabledOpacity, out opacity)))
                newKey.DisabledBackgroundOpacity = opacity;
            else if (newKey.OpacityOverride < 1d)
                newKey.DisabledBackgroundOpacity = newKey.OpacityOverride;

            if (!string.IsNullOrEmpty(xmlKey.KeyDownOpacity) && double.TryParse(xmlKey.KeyDownOpacity, out opacity))
                newKey.KeyDownOpacityOverride = opacity;
            else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.KeyDownOpacity) && double.TryParse(x.KeyDownOpacity, out opacity)))
                newKey.KeyDownOpacityOverride = opacity;
            else if (newKey.OpacityOverride < 1d)
                newKey.KeyDownOpacityOverride = newKey.OpacityOverride;

            if (xmlKeyValue != null && overrideTimesByKey != null)
            {
                TimeSpanOverrides timeSpanOverrides;
                if (xmlKey.LockOnTime >= 0)
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.LockOnTime = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.LockOnTime));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { LockOnTime = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.LockOnTime)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }
                else if (keyGroupList != null && keyGroupList.Exists(x => x.LockOnTime >= 0))
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.LockOnTime = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.LockOnTime >= 0).LockOnTime));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { LockOnTime = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.LockOnTime >= 0).LockOnTime)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }

                if (!string.IsNullOrEmpty (xmlKey.CompletionTimes))
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.CompletionTimes = xmlKey.CompletionTimes.Split(',').ToList(); 
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { CompletionTimes = xmlKey.CompletionTimes.Split(',').ToList() };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }
                else if (keyGroupList != null && keyGroupList.Exists(x => !string.IsNullOrEmpty(x.CompletionTimes)))
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.CompletionTimes = keyGroupList.Find(x => !string.IsNullOrEmpty(x.CompletionTimes)).CompletionTimes.Split(',').ToList();
                    overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { CompletionTimes = keyGroupList.Find(x => !string.IsNullOrEmpty(x.CompletionTimes)).CompletionTimes.Split(',').ToList() };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }

                if (xmlKey.TimeRequiredToLockDown > 0)
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.TimeRequiredToLockDown = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.TimeRequiredToLockDown));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { TimeRequiredToLockDown = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.TimeRequiredToLockDown)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }
                else if (keyGroupList != null && keyGroupList.Exists(x => x.TimeRequiredToLockDown > 0))
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.TimeRequiredToLockDown = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.TimeRequiredToLockDown > 0).TimeRequiredToLockDown));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { TimeRequiredToLockDown = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.TimeRequiredToLockDown > 0).TimeRequiredToLockDown)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }

                if (xmlKey.LockDownAttemptTimeout > 0)
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.LockDownAttemptTimeout = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.LockDownAttemptTimeout));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { LockDownAttemptTimeout = TimeSpan.FromMilliseconds(Convert.ToDouble(xmlKey.LockDownAttemptTimeout)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }
                else if (keyGroupList != null && keyGroupList.Exists(x => x.LockDownAttemptTimeout > 0))
                {
                    if (overrideTimesByKey.TryGetValue(xmlKeyValue, out timeSpanOverrides))
                    {
                        timeSpanOverrides.LockDownAttemptTimeout = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.LockDownAttemptTimeout > 0).LockDownAttemptTimeout));
                        overrideTimesByKey[xmlKeyValue] = timeSpanOverrides;
                    }
                    else
                    {
                        timeSpanOverrides = new TimeSpanOverrides() { LockDownAttemptTimeout = TimeSpan.FromMilliseconds(Convert.ToDouble(keyGroupList.Find(x => x.LockDownAttemptTimeout > 0).LockDownAttemptTimeout)) };
                        overrideTimesByKey.Add(xmlKeyValue, timeSpanOverrides);
                    }
                }
            }
            
            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
        }

        private void SetupKeys()
        {
            XmlKeys keys = keyboard.Keys;

            var allKeys = keys.ActionKeys.Cast<IXmlKey>()
                .Concat(keys.ChangeKeyboardKeys)
                .Concat(keys.DynamicKeys)
                .Concat(keys.PluginKeys)
                .Concat(keys.TextKeys)
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
                newKey.Value = Enum.TryParse(xmlKey.DestinationKeyboard, out Enums.Keyboards keyboardEnum)
                    ? new ChangeKeyboardKeyValue(keyboardEnum, replaceCurrKeyboard)
                    : new ChangeKeyboardKeyValue(Path.Combine(rootDir, xmlKey.DestinationKeyboard), replaceCurrKeyboard);
            }
            else
            {
                Log.ErrorFormat("No destination keyboard found for changekeyboard key with label {0}", xmlKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlKey.Row, xmlKey.Col, xmlKey.Height, xmlKey.Width);
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
            if (ValidColor(keyboard.BorderColor, out var colorBrush))
            {
                Log.InfoFormat("Setting border color for custom keyboard: {0}", keyboard.BorderColor);
                this.BorderBrush = colorBrush;
                if(mainWindow != null)
                {
                    mainWindow.BorderBrushOverride = colorBrush;
                }
            }
            if (ValidColor(keyboard.BackgroundColor, out colorBrush))
            {
                Log.InfoFormat("Setting background color for custom keyboard: {0}", keyboard.BackgroundColor);
                this.Background = colorBrush;
                if (mainWindow != null)
                {
                    mainWindow.BackgroundColourOverride = colorBrush;
                }
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

        private void PlaceKeyInPosition(Key key, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            MainGrid.Children.Add(key);
            Grid.SetColumn(key, col);
            Grid.SetRow(key, row);
            Grid.SetColumnSpan(key, colSpan);
            Grid.SetRowSpan(key, rowSpan);
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

        private bool ValidColor(string color, out SolidColorBrush colorBrush)
        {
            if (!string.IsNullOrEmpty(color)
                && (Regex.IsMatch(color, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$")
                || System.Drawing.Color.FromName(color).IsKnownColor))
            {
                colorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(color);
                return true;
            }
            else
            {
                Log.Error($"Cannot parse string {color} as valid color");
            }
            colorBrush = null;
            return false;
        }
    }
}
