// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services.Suggestions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services.AutoComplete
{
    [TestFixture]
    internal class BasicAutoCompleteTests : AutoCompleteTestsBase
    {
        protected override IManagedSuggestions CreateAutoComplete()
        {
            return new BasicAutoComplete();
		}

		protected override object[] GetTestCases()
		{
			return SuggestionsTestCases;
		}

		private object[] SuggestionsTestCases
		{
            get
            {
                return new object[] {
                    new object[] {
                        "t",
                        new[] {
                            "the", "to", "that", "this", "they", "there", "their", "time", "take", "them", "than",
                            "then", "think", "two", "these"
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
            }
        }
    }
}