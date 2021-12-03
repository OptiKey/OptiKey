// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services.Suggestions;

namespace JuliusSweetland.OptiKey.AutoCompletePerformance
{
    /// <summary>
    ///     Given a mispelt word, start typing it and grab the top 4 suggestions.
    ///     If any of these suggestions is the correct word, then capture:
    ///     * how many characters we entered, or -1 if the correct word never appeared
    ///     * how long this process took
    ///     * the misspelt and target words
    /// </summary>
    internal class SpellingCorrectionTester
    {
        private const string DictionaryFileType = "dic";
        private const int NumberOfSuggestionsToCheck = 4;
        private const string OriginalDictionariesSubPath = @"Dictionaries";
        private readonly IManagedSuggestions managedSuggestion;

        public SpellingCorrectionTester(SuggestionMethods SuggestionMethod, Languages language)
        {
            switch (SuggestionMethod)
            {
                case SuggestionMethods.NGram:
                    managedSuggestion = new NGramAutoComplete();
                    break;
                case SuggestionMethods.Basic:
                    managedSuggestion = new BasicAutoComplete();
                    break;
                case SuggestionMethods.Presage:
                    managedSuggestion = new PresageSuggestions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("SuggestionMethod", SuggestionMethod, null);
            }

            Configure(language);
        }

        public SpellingCorrectionTestResult TestWord(MisspellingTest misspellingTest)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var charactersTyped = -1;
            for (var n = 1; n <= misspellingTest.Misspelling.Length; ++n)
            {
                var typedSoFar = misspellingTest.Misspelling.Substring(0, n);
                var suggestions = managedSuggestion.GetSuggestions(typedSoFar).Take(NumberOfSuggestionsToCheck);

                if (!suggestions.Any(
                        suggestion =>
                            suggestion.Equals(misspellingTest.TargetWord, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                charactersTyped = n;
                break;
            }
            stopwatch.Stop();

            var timeTaken = stopwatch.Elapsed;
            return new SpellingCorrectionTestResult(charactersTyped, timeTaken, misspellingTest);
        }

        private void AddEntryToDictionary(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                return;
            }

            var hash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(false);
            if (string.IsNullOrWhiteSpace(hash))
            {
                return;
            }

            var newEntryWithUsageCount = new DictionaryEntry(entry);

            managedSuggestion.AddEntry(entry, newEntryWithUsageCount);
        }

        private void Configure(Languages keyboardAndDictionaryLanguage)
        {
            var originalDictionaryPath =
                Path.GetFullPath(Path.Combine(OriginalDictionariesSubPath,
                    Path.ChangeExtension(keyboardAndDictionaryLanguage.ToString(), DictionaryFileType)));

            if (File.Exists(originalDictionaryPath))
            {
                LoadOriginalDictionaryFromFile(originalDictionaryPath);
            }
            else
            {
                throw new ApplicationException(string.Format(Resources.DICTIONARY_FILE_NOT_FOUND_ERROR,
                    originalDictionaryPath));
            }
        }

        private void LoadOriginalDictionaryFromFile(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Entries must be londer than 1 character
                    if (!string.IsNullOrWhiteSpace(line) && (line.Trim().Length > 1))
                    {
                        AddEntryToDictionary(line.Trim());
                    }
                }
            }
        }
    }
}