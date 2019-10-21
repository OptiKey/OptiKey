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
using System.Collections.Generic;

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

            if (!ValidateKeyboard()) { return; }

            // New logic for content keyboard
            if (keyboard.Content != null)
            {
                if (!ValidateVisualItems()) { return; }
                SetupGrid(); // Setup all the UI components 
                SetupVisualItems(); 
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

            return true;
        }

        private bool ValidateKeys()
        {
            var allKeys = keyboard.Keys.ActionKeys.Cast<XmlKey>()
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

        private bool ValidateVisualItems()
        {
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

            //begin processing items with a reserved row and column
            var itemPosition = keyboard.Content.Items.ToList();
            foreach (XmlVisualItem visualItem in itemPosition.Where(x => x.Row > -1 && x.Col > -1))
            {
                var vIndex = keyboard.Content.Items.IndexOf(visualItem);
                var vLabel = "Suggestion";
                if (visualItem is XmlDynamicItem dynamicKey) { vLabel = dynamicKey.Label; }
                else if (visualItem is XmlScratchpadItem) { vLabel = "Scratchpad"; }

                if (visualItem.Col + visualItem.Width > keyboard.Grid.Cols || visualItem.Row + visualItem.Height > keyboard.Grid.Rows)
                {
                    SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                        + (vIndex + 1) + " of " + itemPosition.Count
                        + " labeled '" + vLabel
                        + "' at row " + visualItem.Row + " column " + visualItem.Col);
                    return false;
                }
                //find space to allocate and remove it from available
                for (int row = visualItem.Row; row < (visualItem.Row + visualItem.Height); row++)
                {
                    for (int col = visualItem.Col; col < (visualItem.Col + visualItem.Width); col++)
                    {
                        if (!openGrid.ElementAt(row).Exists(x => x.Equals(col))) //if the column is unavailable
                        {
                            SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                                + (vIndex + 1) + " of " + itemPosition.Count
                                + " labeled '" + vLabel
                                + "' at row " + visualItem.Row + " column " + visualItem.Col);
                            return false;
                        }
                        else
                            openGrid.ElementAt(row).Remove(col);
                    }
                }
            }
            //end section 1: processing items with a reserved row and column

            //begin section 2: processing items that need a row and/or column assigned
            //the items are processed in the same order they were listed in the xml file
            //if a row or column is reserved it is handled as an indication to jump forward 
            //to that row or column and mark any/all skipped spaces as empty
            foreach (XmlVisualItem visualItem in itemPosition.Where(x => !(x.Row > -1 && x.Col > -1)))
            {
                var vIndex = itemPosition.IndexOf(visualItem);
                var vLabel = "Suggestion";
                if (visualItem is XmlDynamicItem dynamicKey) { vLabel = dynamicKey.Label; }
                else if (visualItem is XmlScratchpadItem) { vLabel = "Scratchpad"; }

            //find first row with enough available width for the item
            findAvailableRow:
                //if row is reserved then block all preceding columns in all preceding rows
                if (visualItem.Row > -1)
                {
                    for (int row = 0; row < visualItem.Row; row++)
                    {
                        while(openGrid.ElementAt(row).Count() > 0)
                        {
                            openGrid.ElementAt(row).RemoveAt(0);
                        }
                    }
                }
                var vItemColumn = 0;
                var vRowsConfirmed = 0;
                var startRow = openGrid.FindIndex(x => (x.Count() >= visualItem.Width));
                if (startRow < 0 || (visualItem.Row > -1 && startRow > visualItem.Row)) //if not found
                {
                    SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                        + (vIndex + 1) + " of " + itemPosition
                        + " labeled '" + vLabel + "' having width "
                        + visualItem.Width + " and height " + visualItem.Height);
                    return false;
                }
                for (int row = startRow; row < keyboard.Grid.Rows; row++)
                {
                findAvailableColumn:
                    //if column is reserved then block all preceding columns in the row
                    if (visualItem.Col > -1)
                    {
                        while (openGrid.ElementAt(row).First() < visualItem.Col)
                        {
                            openGrid.ElementAt(row).RemoveAt(0);
                        }
                    }
                    //if height > 1 and we are searching subsequent rows then need to start at the confirmed start column
                    var startColumn = (vRowsConfirmed > 0) ? vItemColumn : openGrid.ElementAt(row).First();
                    for (int col = startColumn; col < (startColumn + visualItem.Width); col++)
                    {
                        //if an open space is found remove it from available
                        if (openGrid.ElementAt(row).Exists(x => x == col))
                        {
                            openGrid.ElementAt(row).Remove(col);
                        }
                        //if an open space is not found then we need to reset because the item width has not been satisfied
                        else
                        {
                            vItemColumn = 0;
                            vRowsConfirmed = 0;

                            //if this row has additional space then continue checking this row
                            if (openGrid.ElementAt(row).Count() > 0) { goto findAvailableColumn; }

                            //if this row has no additional space but another row does then advance to that row
                            else { goto findAvailableRow; }
                        }
                    }
                    //if we make it past the column loop then we have a confirmed start column and row
                    vItemColumn = startColumn;
                    vRowsConfirmed++;

                    if (vRowsConfirmed == visualItem.Height)
                    {
                        visualItem.Col = vItemColumn;
                        visualItem.Row = 1 + row - visualItem.Height;
                        break;
                    }
                } //loop back to process the next row

                if (vRowsConfirmed < visualItem.Height)
                {
                    SetupErrorLayout("Invalid keyboard file", "Insufficient space to position item "
                        + (vIndex + 1) + " of " + itemPosition.Count
                        + " labeled '" + vLabel + "' having width "
                        + visualItem.Width + " and height " + visualItem.Height);
                    return false;
                }
                keyboard.Content.Items.RemoveAt(vIndex);
                keyboard.Content.Items.Insert(vIndex, visualItem);
            }
            //end section 2: processing items that need a row and/or column assigned

            return true;
        }

        private void SetupVisualItems()
        { 
            var minKeyWidth = keyboard.Content.Items.Select(k => k.Width).Min();
            var minKeyHeight = keyboard.Content.Items.Select(k => k.Height).Min();

            // Iterate over each item and add to keyboard
            foreach (var visualItem in keyboard.Content.Items)
            {
                if (visualItem is XmlDynamicItem xmlDynamicKey)
                {
                    AddDynamicItem(xmlDynamicKey, minKeyWidth, minKeyHeight);
                }
                else if (visualItem is XmlScratchpadItem xmlScratchpadItem)
                {
                    //TODO: add code to enable background color override for scratchpad & suggestions
                    //if (!string.IsNullOrEmpty(item.BackgroundColor)
                    //   && (Regex.IsMatch(item.BackgroundColor, "^(#[0-9A-Fa-f]{3})$|^(#[0-9A-Fa-f]{6})$")
                    //       || System.Drawing.Color.FromName(item.BackgroundColor).IsKnownColor))
                    //{
                    //    scratchpad.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(item.BackgroundColor);
                    //}

                    var scratchpad = new XmlScratchpad();
                    MainGrid.Children.Add(scratchpad);
                    Grid.SetColumn(scratchpad, visualItem.Col);
                    Grid.SetRow(scratchpad, visualItem.Row);
                    Grid.SetColumnSpan(scratchpad, visualItem.Width);
                    Grid.SetRowSpan(scratchpad, visualItem.Height);
                }
                else if (visualItem is XmlSuggestionRowItem xmlSuggestionRow)
                {
                    var suggestionRow = new XmlSuggestionRow();
                    MainGrid.Children.Add(suggestionRow);
                    Grid.SetColumn(suggestionRow, visualItem.Col);
                    Grid.SetRow(suggestionRow, visualItem.Row);
                    Grid.SetColumnSpan(suggestionRow, visualItem.Width);
                    Grid.SetRowSpan(suggestionRow, visualItem.Height);
                }
                else if (visualItem is XmlSuggestionColItem xmlSuggestionCol)
                {
                    var suggestionCol = new XmlSuggestionCol();
                    MainGrid.Children.Add(suggestionCol);
                    Grid.SetColumn(suggestionCol, visualItem.Col);
                    Grid.SetRow(suggestionCol, visualItem.Row);
                    Grid.SetColumnSpan(suggestionCol, visualItem.Width);
                    Grid.SetRowSpan(suggestionCol, visualItem.Height);
                }
            }
        }

        void AddDynamicItem(XmlDynamicItem xmlDynamicKey, int minKeyWidth, int minKeyHeight)
        {
            Key newKey = CreateKeyFromItem(xmlDynamicKey, minKeyWidth, minKeyHeight);
            if (xmlDynamicKey.Steps.Count > 0)
            {
                var vStepList = "";
                var rootDir = Path.GetDirectoryName(inputFilename);

                foreach (XmlDynamicItem vStep in xmlDynamicKey.Steps)
                {
                    if (vStep is DynamicAction vAction)
                    {
                        //TODO: add code to enable proper functionality 
                        //of keydown an keyup states executed from a dynamic keyboard
                        //also dwell duration and cooldown
                        //also locking behavior
                        FunctionKeys actionEnum;
                        if (!Enum.TryParse(vAction.Value, out actionEnum))
                            Log.ErrorFormat("Could not parse {0} as function key", vAction.Value);
                        else if (actionEnum == FunctionKeys.Sleep
                            || actionEnum == FunctionKeys.LeftShift
                            || actionEnum ==FunctionKeys.LeftCtrl
                            || actionEnum == FunctionKeys.LeftAlt
                            || actionEnum == FunctionKeys.LeftWin)
                        {
                            newKey.Value = new KeyValue(actionEnum);
                            PlaceKeyInPosition(newKey, xmlDynamicKey.Row, xmlDynamicKey.Col, xmlDynamicKey.Height, xmlDynamicKey.Width);
                            return;
                        }
                        else
                            vStepList += "<Action>" + vAction.Value;   
                    }
                    else if (vStep is DynamicLink vLink)
                    {
                        if (string.IsNullOrEmpty(vLink.Value))
                            Log.ErrorFormat("Destination Keyboard not found for {0} ", vLink.Label);
                        else
                        {
                            string vDestinationKeyboard = Enum.TryParse(vLink.Value, out Enums.Keyboards keyboardEnum)
                            ? keyboardEnum.ToString()
                            : Path.Combine(rootDir, vLink.Value);

                            vStepList += (xmlDynamicKey.ReturnToThisKeyboard)
                                ? "<KeyboardAndReturn>" + vDestinationKeyboard
                                : "<Keyboard>" + vDestinationKeyboard;
                        }
                    }
                    else if (vStep is DynamicText vText)
                    {
                        if (string.IsNullOrEmpty(vText.Value))
                            Log.ErrorFormat("Text not found for {0} ", vText.Label);
                        else
                            vStepList += "<Text>" + vText.Value;
                    }
                    else if (vStep is DynamicWait vWait)
                    {
                        int waitInt;
                        if (!int.TryParse(vWait.Value, out waitInt))
                            Log.ErrorFormat("Could not parse wait {0} as int value", vWait.Label);
                        else
                            vStepList += "<Wait>" + vWait.Value;
                    }
                    //TODO:
                    //Plugin commands are written here, but will be 
                    //ignored until a new plugin command handler is written
                    //if (!String.IsNullOrWhiteSpace(vStep.Plugin) && !String.IsNullOrWhiteSpace(vStep.Method))
                    //{
                    //    vStepList += "<Argment>";
                    //    foreach (var vArg in vStep.Argument)
                    //    {
                    //        if (!String.IsNullOrWhiteSpace(vArg.Name))
                    //        {
                    //            vStepList += "<Name>" + vArg.Name;
                    //        }
                    //        if (!String.IsNullOrWhiteSpace(vArg.Value))
                    //        {
                    //            vStepList += "<Value>" + vArg.Value;
                    //        }
                    //    }
                    //    vStepList += "</Argment>";
                    //}
                }

                vStepList = (vStepList == "") ? vStepList : "<StepList>" + vStepList;
                newKey.Value = new KeyValue(FunctionKeys.StepList, vStepList);
            }
            else
            {
                Log.ErrorFormat("No value found in dynamic key with label {0}", xmlDynamicKey.Label);
            }

            PlaceKeyInPosition(newKey, xmlDynamicKey.Row, xmlDynamicKey.Col, xmlDynamicKey.Height, xmlDynamicKey.Width);
        }

        private Key CreateKeyFromItem(XmlDynamicItem xmlKey, int minKeyWidth, int minKeyHeight)
        {
            // Add the core properties from XML to a new key
            Key newKey = new Key();
            if (xmlKey.Label != null)
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

            if (xmlKey.Label != null && xmlKey.ShiftDownLabel != null)
            {
                newKey.ShiftUpText = xmlKey.Label.ToStringWithValidNewlines();
                newKey.ShiftDownText = xmlKey.ShiftDownLabel.ToStringWithValidNewlines();
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
                bool hasString = xmlKey.Label != null || xmlKey.ShiftDownLabel != null;
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
                    var text = newKey.Text != null ? newKey.Text.Compose() : newKey.ShiftDownText?.Compose();

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

        private void SetupKeys()
        {
            XmlKeys keys = keyboard.Keys;

            var allKeys = keys.ActionKeys.Cast<XmlKey>()
                .Concat(keys.ChangeKeyboardKeys.Cast<XmlKey>())
                .Concat(keys.DynamicKeys.Cast<XmlKey>())
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

        private void PlaceKeyInPosition(Key key, int row, int col, int rowspan = 1, int colspan = 1)
        {
            MainGrid.Children.Add(key);
            Grid.SetColumn(key, col);
            Grid.SetRow(key, row);
            Grid.SetColumnSpan(key, colspan);
            Grid.SetRowSpan(key, rowspan);
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