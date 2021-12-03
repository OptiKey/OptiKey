// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.AutoCompletePerformance
{
    public class AutoCorrectTestResults
    {
        public AutoCorrectTestResults(SuggestionMethods autoCompleteMethod,
            IEnumerable<SpellingCorrectionTestResult> results, long memorySize)
        {
            AutoCompleteMethod = autoCompleteMethod;
            Results = results;
            MemorySize = memorySize;
        }

        public SuggestionMethods AutoCompleteMethod { get; private set; }
        public long MemorySize { get; private set; }
        public IEnumerable<SpellingCorrectionTestResult> Results { get; private set; }
    }

    internal class AutoCorrectTester
    {
        private const string SpellingErrorsFile = "misspellings{0}.csv";
        private readonly Languages language;
        private IEnumerable<MisspellingTest> misspellings;

        public AutoCorrectTester(Languages language)
        {
            this.language = language;
        }

        public IEnumerable<AutoCorrectTestResults> Run()
        {
            LoadMisspellings();

            var results = new List<AutoCorrectTestResults>();

            foreach (SuggestionMethods autoCompleteMethod in Enum.GetValues(typeof (SuggestionMethods)))
            {
                var before = GC.GetTotalMemory(true);

                var tester = new SpellingCorrectionTester(autoCompleteMethod, language);
                var testResults = misspellings.Select(tester.TestWord);

                var after = GC.GetTotalMemory(false);

                results.Add(new AutoCorrectTestResults(autoCompleteMethod, testResults, after - before));
            }

            return results;
        }

        private void LoadMisspellings()
        {
            var source =
                File.ReadAllLines(string.Format(SpellingErrorsFile, language)).Skip(1).Select(line => line.Split(','));
            misspellings = source.Select(strings => new MisspellingTest(strings[0].Trim(), strings[1].Trim()));
        }
    }
}