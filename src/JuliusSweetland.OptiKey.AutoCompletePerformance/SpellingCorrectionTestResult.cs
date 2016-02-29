using System;

namespace JuliusSweetland.OptiKey.AutoCompletePerformance
{
    public struct SpellingCorrectionTestResult
    {
        private readonly MisspellingTest misspellingTest;

        public SpellingCorrectionTestResult(int charactersTyped, TimeSpan timeTaken, MisspellingTest misspellingTest)
        {
            CharactersTyped = charactersTyped;
            TimeTaken = timeTaken;
            this.misspellingTest = misspellingTest;
        }

        public int CharactersTyped { get; private set; }
        public TimeSpan TimeTaken { get; private set; }

        public string Misspelling
        {
            get { return misspellingTest.Misspelling; }
        }

        public string TargetWord
        {
            get { return misspellingTest.TargetWord; }
        }
    }
}