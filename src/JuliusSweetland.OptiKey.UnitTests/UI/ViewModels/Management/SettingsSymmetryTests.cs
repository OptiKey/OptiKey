// Copyright (c) 2026 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.Management
{
    /// <summary>
    /// Tests that ensure all settings loaded in Load() are saved in ApplyChanges() and vice versa.
    /// This prevents bugs where a setting can be displayed but changes are silently discarded.
    /// This test automatically generates tests for each ViewModel in the Management Console
    /// </summary>
    [TestFixture]
    public class SettingsSymmetryTests
    {
        // Pattern for reading settings (any access to Settings.Default.PropertyName)
        private static readonly Regex SettingsReadPattern =
            new Regex(@"Settings\.Default\.(\w+)", RegexOptions.Compiled);

        // Pattern for writing settings (Settings.Default.PropertyName = ..., but not == or !=)
        private static readonly Regex SettingsWritePattern =
            new Regex(@"Settings\.Default\.(\w+)\s*=(?!=)", RegexOptions.Compiled);

        // Known exceptions: settings that are intentionally asymmetric
        // For example, if they are computed indirectly or saved via a function.
        // Add property names here if you *know* they are correctly handled
        private static readonly HashSet<string> KnownExceptions = new HashSet<string>
        {
            // Computed from other settings in WordsViewModel
            "UseCommuniKateKeyboardLayoutByDefault",
            "UsingCommuniKateKeyboardLayout",
            // Conditionally saved based on EnableOverrideTranslationApiKey in FeaturesViewModel
            "OverriddenTranslationApiKey",
            // Set but not read directly (triggers update flag)
            "EyeGestureUpdated",
            // Saved indirectly via windowManipulationService.ChangeState() in VisualsViewModel
            "MainWindowState",
            "MainWindowDockPosition"
        };

        private static string GetViewModelsPath()
        {
            // Navigate from test output directory to source
            var testDir = TestContext.CurrentContext.TestDirectory;
            var solutionDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
            return Path.Combine(solutionDir,
                "JuliusSweetland.OptiKey.Core", "UI", "ViewModels", "Management");
        }

        private static IEnumerable<TestCaseData> GetViewModelFiles()
        {
            var viewModelsPath = GetViewModelsPath();

            if (!Directory.Exists(viewModelsPath))
            {
                yield break;
            }

            var files = Directory.GetFiles(viewModelsPath, "*ViewModel.cs");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                // Only include files that have both Load and ApplyChanges methods
                if (content.Contains("void Load()") && content.Contains("void ApplyChanges()"))
                {
                    yield return new TestCaseData(file)
                        .SetName($"SettingsSymmetry_{Path.GetFileNameWithoutExtension(file)}");
                }
            }
        }

        [TestCaseSource(nameof(GetViewModelFiles))]
        public void ViewModel_LoadAndApplyChanges_ShouldHaveSymmetricSettingsAccess(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            var loadBody = ExtractMethodBody(content, "Load");
            var applyBody = ExtractMethodBody(content, "ApplyChanges");

            Assert.That(loadBody, Is.Not.Null,
                $"Could not extract Load() method body from {fileName}");
            Assert.That(applyBody, Is.Not.Null,
                $"Could not extract ApplyChanges() method body from {fileName}");

            // Extract settings read in Load() and written in ApplyChanges()
            var loadedSettings = ExtractSettingsNames(loadBody, SettingsReadPattern);
            var savedSettings = ExtractSettingsNames(applyBody, SettingsWritePattern);

            // Find asymmetries (excluding known exceptions)
            var loadedButNotSaved = loadedSettings
                .Except(savedSettings)
                .Except(KnownExceptions)
                .ToList();

            var savedButNotLoaded = savedSettings
                .Except(loadedSettings)
                .Except(KnownExceptions)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(loadedButNotSaved, Is.Empty,
                    $"{fileName}: Settings loaded but not saved: {string.Join(", ", loadedButNotSaved)}");
                Assert.That(savedButNotLoaded, Is.Empty,
                    $"{fileName}: Settings saved but not loaded: {string.Join(", ", savedButNotLoaded)}");
            });
        }

        private static string ExtractMethodBody(string content, string methodName)
        {
            // Match both private and public void methods
            var pattern = $@"(private|public)\s+void\s+{methodName}\s*\(\s*\)\s*\{{";
            var match = Regex.Match(content, pattern);

            if (!match.Success)
                return null;

            int start = match.Index + match.Length;
            int braceCount = 1;
            int end = start;

            while (braceCount > 0 && end < content.Length)
            {
                if (content[end] == '{') braceCount++;
                else if (content[end] == '}') braceCount--;
                end++;
            }

            return content.Substring(start, end - start - 1);
        }

        private static HashSet<string> ExtractSettingsNames(string methodBody, Regex pattern)
        {
            var matches = pattern.Matches(methodBody);
            return new HashSet<string>(
                matches.Cast<Match>().Select(m => m.Groups[1].Value));
        }
    }
}
