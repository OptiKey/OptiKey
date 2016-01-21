using System.Linq;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services.AutoComplete;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
                new {Word = "the", Count = 100}, new {Word = "be", Count = 99}, new {Word = "to", Count = 98},
                new {Word = "of", Count = 97}, new {Word = "and", Count = 96}, new {Word = "a", Count = 95},
                new {Word = "in", Count = 94}, new {Word = "that", Count = 93}, new {Word = "have", Count = 92},
                new {Word = "I", Count = 91}, new {Word = "it", Count = 90}, new {Word = "for", Count = 89},
                new {Word = "not", Count = 88}, new {Word = "on", Count = 87}, new {Word = "with", Count = 86},
                new {Word = "he", Count = 85}, new {Word = "as", Count = 84}, new {Word = "you", Count = 83},
                new {Word = "do", Count = 82}, new {Word = "at", Count = 81}, new {Word = "this", Count = 80},
                new {Word = "but", Count = 79}, new {Word = "his", Count = 78}, new {Word = "by", Count = 77},
                new {Word = "from", Count = 76}, new {Word = "they", Count = 75}, new {Word = "we", Count = 74},
                new {Word = "say", Count = 73}, new {Word = "her", Count = 72}, new {Word = "she", Count = 71},
                new {Word = "or", Count = 70}, new {Word = "an", Count = 69}, new {Word = "will", Count = 68},
                new {Word = "my", Count = 67}, new {Word = "one", Count = 66}, new {Word = "all", Count = 65},
                new {Word = "would", Count = 64}, new {Word = "there", Count = 63}, new {Word = "their", Count = 62},
                new {Word = "what", Count = 61}, new {Word = "so", Count = 60}, new {Word = "up", Count = 59},
                new {Word = "out", Count = 58}, new {Word = "if", Count = 57}, new {Word = "about", Count = 56},
                new {Word = "who", Count = 55}, new {Word = "get", Count = 54}, new {Word = "which", Count = 53},
                new {Word = "go", Count = 52}, new {Word = "me", Count = 51}, new {Word = "when", Count = 50},
                new {Word = "make", Count = 49}, new {Word = "can", Count = 48}, new {Word = "like", Count = 47},
                new {Word = "time", Count = 46}, new {Word = "no", Count = 45}, new {Word = "just", Count = 44},
                new {Word = "him", Count = 43}, new {Word = "know", Count = 42}, new {Word = "take", Count = 41},
                new {Word = "people", Count = 40}, new {Word = "into", Count = 39}, new {Word = "year", Count = 38},
                new {Word = "your", Count = 37}, new {Word = "good", Count = 36}, new {Word = "some", Count = 35},
                new {Word = "could", Count = 34}, new {Word = "them", Count = 33}, new {Word = "see", Count = 32},
                new {Word = "other", Count = 31}, new {Word = "than", Count = 30}, new {Word = "then", Count = 29},
                new {Word = "now", Count = 28}, new {Word = "look", Count = 27}, new {Word = "only", Count = 26},
                new {Word = "come", Count = 25}, new {Word = "its", Count = 24}, new {Word = "over", Count = 23},
                new {Word = "think", Count = 22}, new {Word = "also", Count = 21}, new {Word = "back", Count = 20},
                new {Word = "after", Count = 19}, new {Word = "use", Count = 18}, new {Word = "two", Count = 17},
                new {Word = "how", Count = 16}, new {Word = "our", Count = 15}, new {Word = "work", Count = 14},
                new {Word = "first", Count = 13}, new {Word = "well", Count = 12}, new {Word = "way", Count = 11},
                new {Word = "even", Count = 10}, new {Word = "new", Count = 9}, new {Word = "want", Count = 8},
                new {Word = "because", Count = 7}, new {Word = "any", Count = 6}, new {Word = "these", Count = 5},
                new {Word = "give", Count = 4}, new {Word = "day", Count = 3}, new {Word = "most", Count = 2},
                new {Word = "us", Count = 1}
            };

            foreach (var entry in entries)
            {
                AddEntry(entry.Word, entry.Count);
            }
        }

        private void AddEntry(string entry, int count)
        {
            basicAutoComplete.AddEntry(entry, new DictionaryEntry(entry, count));
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
            basicAutoComplete.AddEntry("these", new DictionaryEntry("these", 101));

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
            basicAutoComplete.AddEntry("zoo", new DictionaryEntry("zoo"));

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