// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.IO;
using System.Text;
using CsvHelper;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.AutoCompletePerformance
{
    internal static class Program
    {
        private static void Main()
        {
            using (var csv = new CsvWriter(new StreamWriter("results.csv", false, Encoding.UTF8)))
            {
                WriteHeader(csv);

                var languagesToTest = new[] {Languages.EnglishUK, Languages.GermanGermany};
                foreach (var language in languagesToTest)
                {
                    var results = new AutoCorrectTester(language).Run();

                    foreach (var result in results)
                    {
                        foreach (var testResult in result.Results)
                        {
                            csv.WriteField(language);
                            csv.WriteField(result.AutoCompleteMethod);
                            csv.WriteField(result.MemorySize);
                            csv.WriteField(testResult.Misspelling);
                            csv.WriteField(testResult.TargetWord);
                            csv.WriteField(testResult.CharactersTyped);
                            csv.WriteField(testResult.TargetWord.Length);
                            csv.WriteField(testResult.TimeTaken.Milliseconds);
                            csv.WriteField(testResult.CharactersTyped != -1
                                ? (double) testResult.CharactersTyped/testResult.TargetWord.Length
                                : 2d);
                            csv.NextRecord();
                        }
                    }
                }
            }
        }

        private static void WriteHeader(CsvWriter csv)
        {
            csv.WriteField("Language");
            csv.WriteField("Method");
            csv.WriteField("MemoryUsedForTestSuite");
            csv.WriteField("Misspelling");
            csv.WriteField("Target");
            csv.WriteField("CharactersTyped");
            csv.WriteField("TargetLength");
            csv.WriteField("TimeTakenMs");
            csv.WriteField("%ageTyped");
            csv.NextRecord();
        }
    }
}