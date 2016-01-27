using System.Linq;
using JuliusSweetland.OptiKey.Services.AutoComplete;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services.AutoComplete
{
    [TestFixture]
    public class NGramAutoCompleteTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Arrange()
        {
            ngramAutoComplete = new NGramAutoComplete();
        }

        #endregion

        private NGramAutoComplete ngramAutoComplete;

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
                ngramAutoComplete.AddEntry(entries[index], 100 - index);
            }
        }

        private static readonly object[] SuggestionsTestCaseSource = {
            new object[] {
                "t",
                new[] {
                    "to", "the", "two", "that", "this", "they", "time", "take", "them", "than", "then", "there", "their",
                    "think", "these"
                }
            },
            new object[] {
                "th",
                new[] {
                    "the", "that", "this", "they", "them", "than", "then", "there", "their", "think", "these", "to",
                    "two", "with", "time", "take"
                }
            },
            new object[] {
                "the",
                new[] {
                    "the", "they", "them", "then", "there", "their", "these", "that", "this", "than", "think", "to",
                    "he", "she", "two", "time", "take", "other"
                }
            },
            new object[] {
                "thes",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "two", "time", "take", "other"
                }
            },
            new object[] {
                "thesa",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "two", "time", "take", "other"
                }
            },
            new object[] {
                "thesau",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "two", "time", "take", "other"
                }
            },
            new object[] {
                "thesaur",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "two", "our", "time", "take", "your", "other"
                }
            },
            new object[] {
                "thesauru",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "two", "time", "take", "other"
                }
            },
            new object[] {
                "thesaurus",
                new[] {
                    "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think", "to",
                    "us", "two", "time", "take", "other"
                }
            }
        };

        private void ExpectEmpty(string root)
        {
            var suggestions = ngramAutoComplete.GetSuggestions(root).ToList();

            CollectionAssert.IsEmpty(suggestions);
        }

        private void TestGetSuggestions(string root, string[] expectedSuggestions)
        {
            var suggestions = ngramAutoComplete.GetSuggestions(root).ToList();

            CollectionAssert.AreEqual(expectedSuggestions, suggestions);
        }

        [Test]
        public void AddEntry_called_with_existing_entry_does_not_update_usage_count()
        {
            ConfigureProvider();

            // try to make this the "t"-word with the highest usage
            ngramAutoComplete.AddEntry("these", 101);

            var suggestions = ngramAutoComplete.GetSuggestions("t");

            Assert.AreNotEqual("these", suggestions.First());
        }

        [Test]
        public void After_AddEntry_called_provider_will_return_word_as_suggestion()
        {
            ngramAutoComplete.AddEntry("zoo");

            var suggestions = ngramAutoComplete.GetSuggestions("z");

            CollectionAssert.Contains(suggestions, "zoo");
        }

        [Test]
        public void Clear_on_a_configured_provider_removes_all_suggestions()
        {
            const string root = "t";
            ConfigureProvider();

            ngramAutoComplete.Clear();

            ExpectEmpty(root);
        }

        [Test]
        public void Clear_on_an_empty_provider_succeeds()
        {
            ngramAutoComplete.Clear();
        }

        [Test]
        [TestCaseSource("SuggestionsTestCaseSource")]
        public void GetSuggestions_returns_words_containing_matching_trigrams(string root,
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

            ngramAutoComplete.RemoveEntry("people");

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

            ngramAutoComplete.RemoveEntry("thesaurus");

            TestGetSuggestions(root, expectedSuggestions);
        }
    }
}