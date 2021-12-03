// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services.Suggestions;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services.AutoComplete
{
    internal abstract class AutoCompleteTestsBase
    {
        private IManagedSuggestions autoComplete;
		protected static object[] SuggestionsTestCaseSource { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }

        [Test]
        public void AddEntry_called_with_existing_entry_does_not_update_usage_count()
        {
            ConfigureProvider();

            // try to make this the "t"-word with the highest usage
            autoComplete.AddEntry("these", new DictionaryEntry("these", 101));

            var suggestions = autoComplete.GetSuggestions("t");

            Assert.That(suggestions.First(), Is.Not.EqualTo("these"));
        }

        [Test]
        public void After_AddEntry_called_provider_will_return_word_as_suggestion()
        {
            autoComplete.AddEntry("zoo", new DictionaryEntry("zoo"));

            var suggestions = autoComplete.GetSuggestions("z");

            Assert.That(suggestions, Contains.Item("zoo"));
        }

        [Test]
        public void Clear_on_a_configured_provider_removes_all_suggestions()
        {
            const string root = "t";
            ConfigureProvider();

            autoComplete.Clear();

            ExpectEmpty(root);
        }

        [Test]
        public void Clear_on_an_empty_provider_succeeds()
        {
            autoComplete.Clear();
        }

        [Test]
        public void GetSuggestions_returns_phrases_matching_input_initials()
        {
            ConfigureProviderForPhrases();

            var suggestions = autoComplete.GetSuggestions("hay").Take(4).ToList();

            Assert.That(suggestions, Contains.Item("how are you"));
        }

        [Test]
        [TestCaseSource("SuggestionsTestCaseSource")]
        public void GetSuggestions_returns_the_expected_words(string root,
            string[] expectedSuggestions)
        {
            ConfigureProvider();

            TestGetSuggestions(root, expectedSuggestions);
        }

        [Test]
        public void RemoveEntry_ensures_a_word_is_no_longer_returned_as_a_suggestion()
        {
            const string root = "peo";
            ConfigureProvider();

            autoComplete.RemoveEntry("people");

            // "peo" is not a word, so the list will either be 0 or 1 long, as we've not populated any other words
            // beginning with those letters.
            ExpectEmpty(root);
        }

        [Test]
        [TestCaseSource("SuggestionsTestCaseSource")]
        public void RemoveEntry_on_a_word_not_in_the_provider_succeeds(string root,
            string[] expectedSuggestions)
        {
            ConfigureProvider();

            autoComplete.RemoveEntry("thesaurus");

            TestGetSuggestions(root, expectedSuggestions);
        }


        protected abstract IManagedSuggestions CreateAutoComplete();
		protected abstract object[] GetTestCases();

		/// <remarks>Top 100 most common words in English: https://en.wikipedia.org/wiki/Most_common_words_in_English. </remarks>
		private void ConfigureProvider()
        {
            var entries = new[] {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "I", "it", "for", "not", "on", "with", "he",
                "as", "you", "do", "at", "this", "but", "his", "by", "from", "they", "we", "say", "her", "she", "or",
                "an", "will", "my", "one", "all", "would", "there", "their", "what", "so", "up", "out", "if", "about",
                "who", "get", "which", "go", "me", "when", "make", "can", "like", "time", "no", "just", "him", "know",
                "take", "people", "into", "year", "your", "good", "some", "could", "them", "see", "other", "than",
                "then", "now", "look", "only", "come", "its", "over", "think", "also", "back", "after", "use", "two",
                "how", "our", "work", "first", "well", "way", "even", "new", "want", "because", "any", "these", "give",
                "day", "most", "us"
            };
            for (var index = 0; index < entries.Length; index++)
            {
                var word = entries[index];
                autoComplete.AddEntry(word, new DictionaryEntry(word, 100 - index));
            }
        }

        private void ConfigureProviderForPhrases()
        {
            var entries = new[] {
                "how are you", "hay", "hays", "hay's", "hayed", "ha", "haying", "had", "hag", "halfway", "hallway",
                "ham", "has", "hat", "haywire", "haystack", "hack", "hags", "hail", "hair", "hale"
            };
            for (var index = 0; index < entries.Length; index++)
            {
                var word = entries[index];
                autoComplete.AddEntry(word, new DictionaryEntry(word, 100 - index));
            }
        }

        private void ExpectEmpty(string root)
        {
            var suggestions = autoComplete.GetSuggestions(root);

            Assert.That(suggestions, Is.Empty);
        }

        private void TestGetSuggestions(string root, IEnumerable<string> expectedSuggestions)
        {
            var suggestions = autoComplete.GetSuggestions(root);
                        
            Assert.That(suggestions, Is.EqualTo(expectedSuggestions));
        }

        #region Setup/Teardown

        [SetUp]
        public void Arrange() {
            autoComplete = CreateAutoComplete();
			SuggestionsTestCaseSource = GetTestCases();
		}

        #endregion
    }
}
