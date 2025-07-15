using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Enums;
using log4net;

namespace KeyboardValidator
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static int Main(string[] args)
        {
            // Configure logging to console
            log4net.Config.BasicConfigurator.Configure();

            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: KeyboardValidator <keyboard_file_path>");
                Console.Error.WriteLine("Example: KeyboardValidator MyKeyboard.xml");
                return 1;
            }

            string keyboardPath = args[0];
            
            try
            {
                var validator = new DynamicKeyboardValidator();
                bool isValid = validator.ValidateKeyboard(keyboardPath);
                return isValid ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Validation failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return 1;
            }
        }
    }

    public class DynamicKeyboardValidator
    {
        public bool ValidateKeyboard(string inputFilename)
        {
            Console.WriteLine($"Validating keyboard: {inputFilename}");
            
            // Load the XML keyboard file using OptiKey's loader
            XmlKeyboard keyboard;
            try
            {
                keyboard = XmlKeyboard.ReadFromFile(inputFilename);
                Console.WriteLine("✓ XML file loaded successfully");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"✗ Error loading keyboard file: {e.Message}");
                return false;
            }

            // Validate the keyboard structure (same logic as OptiKey DynamicKeyboard.xaml.cs)
            if (!ValidateKeyboardStructure(keyboard))
            {
                return false;
            }

            // Validate keys if using legacy format
            if (keyboard.Keys != null)
            {
                if (!ValidateKeys(keyboard))
                {
                    return false;
                }
                Console.WriteLine("✓ Legacy keys validation passed");
            }

            // Validate dynamic content if using new format
            if (keyboard.Content != null)
            {
                if (!ValidateDynamicContent(keyboard))
                {
                    return false;
                }
                Console.WriteLine("✓ Dynamic content validation passed");
            }

            Console.WriteLine("✓ Keyboard validation successful!");
            return true;
        }

        private bool ValidateKeyboardStructure(XmlKeyboard keyboard)
        {
            // Validate window properties (from DynamicKeyboard.xaml.cs ValidateKeyboard method)
            if (!string.IsNullOrWhiteSpace(keyboard.WindowState) && 
                Enum.TryParse(keyboard.WindowState, out WindowStates validWindowState) &&
                validWindowState != WindowStates.Docked && 
                validWindowState != WindowStates.Floating && 
                validWindowState != WindowStates.Maximised)
            {
                Console.Error.WriteLine("✗ WindowState not valid");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(keyboard.Position) && 
                !Enum.TryParse<MoveToDirections>(keyboard.Position, out _))
            {
                Console.Error.WriteLine("✗ Position not valid");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(keyboard.DockSize) && 
                !Enum.TryParse<DockSizes>(keyboard.DockSize, out _))
            {
                Console.Error.WriteLine("✗ DockSize not valid");
                return false;
            }

            // Validate numeric properties
            if (!ValidateNumericProperty(keyboard.Width, "Width")) return false;
            if (!ValidateNumericProperty(keyboard.Height, "Height")) return false;
            if (!ValidateNumericProperty(keyboard.HorizontalOffset, "HorizontalOffset")) return false;
            if (!ValidateNumericProperty(keyboard.VerticalOffset, "VerticalOffset")) return false;

            // Validate grid
            if (keyboard.Grid == null)
            {
                Console.Error.WriteLine("✗ No grid definition found");
                return false;
            }

            if (keyboard.Grid.Rows < 1 || keyboard.Grid.Cols < 1)
            {
                Console.Error.WriteLine($"✗ Grid size is {keyboard.Grid.Rows} rows and {keyboard.Grid.Cols} columns");
                return false;
            }

            // Ensure there's content
            if ((keyboard.Keys == null || keyboard.Keys.Count == 0) && keyboard.Content == null)
            {
                Console.Error.WriteLine("✗ No key or content definitions found");
                return false;
            }

            Console.WriteLine("✓ Keyboard structure validation passed");
            return true;
        }

        private bool ValidateNumericProperty(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;

            if (double.TryParse(value.Replace("%", ""), out var number))
            {
                if (number >= -9999 && number <= 9999)
                {
                    return true;
                }
            }

            Console.Error.WriteLine($"✗ {propertyName} must be between -9999 and 9999");
            return false;
        }

        private bool ValidateKeys(XmlKeyboard keyboard)
        {
            var allKeys = keyboard.Keys.ActionKeys.Cast<IXmlKey>()
                .Concat(keyboard.Keys.ChangeKeyboardKeys)
                .Concat(keyboard.Keys.DynamicKeys)
                .Concat(keyboard.Keys.PluginKeys)
                .Concat(keyboard.Keys.TextKeys)
                .ToList();

            // Check for duplicate positions (from DynamicKeyboard.xaml.cs ValidateKeys method)
            var duplicates = allKeys
                .GroupBy(key => new Tuple<int, int>(key.Row, key.Col))
                .Where(group => group.Count() > 1)
                .Select(group => group.ToList())
                .ToList();

            if (duplicates.Count > 0)
            {
                var errorMsg = duplicates.Select(keys =>
                {
                    var keyStrings = keys.Select(GetKeyString).Aggregate((seq, next) => $"{seq}, {next}");
                    return $"{keyStrings} ({keys.First().Row}, {keys.First().Col})";
                }).Aggregate((msg, key) => $"{msg}, {key}");

                Console.Error.WriteLine($"✗ Duplicate row/column values for keys: {errorMsg}");
                return false;
            }

            // Validate keys fit within grid bounds
            foreach (var key in allKeys)
            {
                if (key.Row < 0 || key.Row >= keyboard.Grid.Rows ||
                    key.Col < 0 || key.Col >= keyboard.Grid.Cols)
                {
                    Console.Error.WriteLine($"✗ Key {GetKeyString(key)} at ({key.Row}, {key.Col}) is outside grid bounds (0-{keyboard.Grid.Rows-1}, 0-{keyboard.Grid.Cols-1})");
                    return false;
                }

                if (key.Row + key.Height > keyboard.Grid.Rows ||
                    key.Col + key.Width > keyboard.Grid.Cols)
                {
                    Console.Error.WriteLine($"✗ Key {GetKeyString(key)} at ({key.Row}, {key.Col}) with size ({key.Height}, {key.Width}) extends beyond grid bounds");
                    return false;
                }
            }

            Console.WriteLine($"✓ Validated {allKeys.Count} keys for position conflicts and grid boundaries");
            return true;
        }

        private bool ValidateDynamicContent(XmlKeyboard keyboard)
        {
            if (keyboard.Content?.Items == null || keyboard.Content.Items.Count == 0)
            {
                Console.WriteLine("✓ No dynamic content items to validate");
                return true;
            }

            // Simulate the OptiKey dynamic content validation logic
            var minKeyWidth = keyboard.Content.Items.Select(k => k.Width).Min() > 0 ? keyboard.Content.Items.Select(k => k.Width).Min() : 1;
            var minKeyHeight = keyboard.Content.Items.Select(k => k.Height).Min() > 0 ? keyboard.Content.Items.Select(k => k.Height).Min() : 1;

            // Track grid space allocation like OptiKey does
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

            // Process items with reserved positions first
            var itemsWithPositions = keyboard.Content.Items.Where(x => x.Row > -1 && x.Col > -1).ToList();
            foreach (var item in itemsWithPositions)
            {
                var itemIndex = keyboard.Content.Items.IndexOf(item);
                var itemLabel = GetDynamicItemLabel(item, itemIndex);

                // Check if item fits in grid
                if (item.Col + item.Width > keyboard.Grid.Cols || item.Row + item.Height > keyboard.Grid.Rows)
                {
                    Console.Error.WriteLine($"✗ Insufficient space to position item {itemIndex + 1} of {keyboard.Content.Items.Count}{itemLabel} at row {item.Row} column {item.Col}");
                    return false;
                }

                // Check for overlapping items
                for (int row = item.Row; row < (item.Row + item.Height); row++)
                {
                    for (int col = item.Col; col < (item.Col + item.Width); col++)
                    {
                        if (!openGrid[row].Contains(col))
                        {
                            Console.Error.WriteLine($"✗ Item {itemIndex + 1} of {keyboard.Content.Items.Count}{itemLabel} at row {item.Row} column {item.Col} overlaps with another item");
                            return false;
                        }
                        openGrid[row].Remove(col);
                    }
                }
            }

            Console.WriteLine($"✓ Validated {keyboard.Content.Items.Count} dynamic content items for positioning and overlaps");
            return true;
        }

        private string GetKeyString(IXmlKey xmlKey)
        {
            if (xmlKey is XmlTextKey textKey)
                return textKey.Text ?? textKey.Label ?? textKey.Symbol ?? "Unknown";

            return xmlKey.Label ?? xmlKey.Symbol ?? "Unknown";
        }

        private string GetDynamicItemLabel(XmlDynamicItem item, int index)
        {
            if (item is XmlDynamicKey dynamicKey)
            {
                return !string.IsNullOrEmpty(dynamicKey.Label) ? $" with label '{dynamicKey.Label}'"
                    : !string.IsNullOrEmpty(dynamicKey.Symbol) ? $" with symbol '{dynamicKey.Symbol}'"
                    : " with no label or symbol";
            }
            else if (item is XmlDynamicScratchpad)
            {
                return " with type of Scratchpad";
            }
            else
            {
                return " with type of Suggestion";
            }
        }
    }
}