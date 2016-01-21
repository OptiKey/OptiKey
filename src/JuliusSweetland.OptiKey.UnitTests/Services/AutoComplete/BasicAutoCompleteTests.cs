using System.Linq;
using JuliusSweetland.OptiKey.Services.AutoComplete;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services.AutoComplete
{
    [TestFixture]
    public class BasicAutoCompleteTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Arrange()
        {
            basicAutoComplete = new BasicAutoComplete();
        }

        #endregion

        private BasicAutoComplete basicAutoComplete;

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
                basicAutoComplete.AddEntry(entries[index], 100 - index);
            }
        }

        private static readonly object[] SuggestionsTestCaseSource = {
            new object[] {
                "t",
                new[] {
                    "the", "to", "that", "this", "they", "there", "their", "time", "take", "them", "than", "then",
                    "think", "two", "these"
                }
            },
            new object[] {
                "th",
                new[] {
                    "the", "that", "this", "they", "there", "their", "them", "than", "then", "think", "these"
                }
            },
            new object[] {
                "the",
                new[] {
                    "the", "they", "there", "their", "them", "then", "these"
                }
            },
            new object[] {
                "thes",
                new[] {
                    "these"
                }
            },
            new object[] {
                "thesa",
                new string[] {}
            }
        };

        private void ExpectEmptyOrRootOnly(string root)
        {
            var suggestions = basicAutoComplete.GetSuggestions(root).ToList();

            Assert.IsTrue((suggestions.Count == 0) || (suggestions.Count == 1));

            var firstOrDefault = suggestions.FirstOrDefault();
            Assert.IsTrue((firstOrDefault == null) || (firstOrDefault == root));
        }

        private void TestGetSuggestions(string root, string[] expectedSuggestions)
        {
            var suggestions = basicAutoComplete.GetSuggestions(root).ToList();

            // The first suggestion can be the original root input.
            var first = suggestions.First();
            if ((first == root) && ((expectedSuggestions.Length == 0) || (root != expectedSuggestions[0])))
            {
                suggestions.RemoveAt(0);
            }
            CollectionAssert.AreEqual(expectedSuggestions, suggestions);
        }

        [Test]
        public void AddEntry_called_with_existing_entry_does_not_update_usage_count()
        {
            ConfigureProvider();

            // try to make this the "t"-word with the highest usage
            basicAutoComplete.AddEntry("these", 101);

            var suggestions = basicAutoComplete.GetSuggestions("t").ToList();

            var suggestion = suggestions[0];
            if (suggestion == "t")
            {
                suggestion = suggestions[1];
            }

            Assert.AreNotEqual("these", suggestion);
        }

        [Test]
        public void After_AddEntry_called_provider_will_return_word_as_suggestion()
        {
            basicAutoComplete.AddEntry("zoo");

            var suggestions = basicAutoComplete.GetSuggestions("z");

            CollectionAssert.Contains(suggestions, "zoo");
        }

        [Test]
        public void Clear_on_a_configured_provider_removes_all_suggestions_and_may_only_return_root_input()
        {
            const string root = "t";
            ConfigureProvider();

            basicAutoComplete.Clear();

            ExpectEmptyOrRootOnly(root);
        }

        [Test]
        public void Clear_on_an_empty_provider_succeeds()
        {
            basicAutoComplete.Clear();
        }

        [Test]
        [TestCaseSource("SuggestionsTestCaseSource")]
        public void GetSuggestions_returns_words_matching_the_root_prefix_in_descending_usage_order(string root,
            string[] expectedSuggestions)
        {
            ConfigureProvider();

            TestGetSuggestions(root, expectedSuggestions);
        }

        [Test]
        public void RemoveEntry_ensures_a_word_is_no_longer_returned_as_a_suggestion_unless_it_is_the_input_root()
        {
            const string root = "peo";
            ConfigureProvider();

            basicAutoComplete.RemoveEntry("people");

            // "peo" is not a word, so the list will either be 0 or 1 long, as we've not populated any other words
            // beginning with those letters.
            ExpectEmptyOrRootOnly(root);
        }

        [Test]
        [TestCaseSource("SuggestionsTestCaseSource")]
        public void RemoveEntry_on_a_word_not_in_the_provider_succeeds(string root,
            string[] expectedSuggestions)
        {
            ConfigureProvider();

            basicAutoComplete.RemoveEntry("thesaurus");

            TestGetSuggestions(root, expectedSuggestions);
        }
    }
}