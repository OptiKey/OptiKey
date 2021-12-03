// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.AutoCompletePerformance
{
    public struct MisspellingTest
    {
        public MisspellingTest(string misspelling, string targetWord)
        {
            Misspelling = misspelling;
            TargetWord = targetWord;
        }

        public string Misspelling { get; }

        public string TargetWord { get; }
    }
}