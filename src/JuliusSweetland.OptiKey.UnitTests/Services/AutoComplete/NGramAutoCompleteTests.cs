// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services.Suggestions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services.AutoComplete
{
    [TestFixture]
    internal class NGramAutoCompleteTests : AutoCompleteTestsBase
    {
        protected override IManagedSuggestions CreateAutoComplete()
        {
            return new NGramAutoComplete();
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
                            "to", "the", "two", "that", "this", "they", "time", "take", "them", "than", "then", "there",
                            "their", "think", "these"
                        }
                    },
                    new object[] {
                        "th",
                        new[] {
                            "the", "that", "this", "they", "them", "than", "then", "there", "their", "think", "these",
                            "to", "two", "with", "time", "take"
                        }
                    },
                    new object[] {
                        "the",
                        new[] {
                            "the", "they", "them", "then", "there", "their", "these", "that", "this", "than", "think",
                            "to", "he", "she", "two", "time", "take", "other"
                        }
                    },
                    new object[] {
                        "thes",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "two", "time", "take", "other"
                        }
                    },
                    new object[] {
                        "thesa",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "two", "time", "take", "other"
                        }
                    },
                    new object[] {
                        "thesau",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "two", "time", "take", "other"
                        }
                    },
                    new object[] {
                        "thesaur",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "two", "our", "time", "take", "your", "other"
                        }
                    },
                    new object[] {
                        "thesauru",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "two", "time", "take", "other"
                        }
                    },
                    new object[] {
                        "thesaurus",
                        new[] {
                            "these", "the", "they", "them", "then", "there", "their", "that", "this", "than", "think",
                            "to", "us", "two", "time", "take", "other"
                        }
                    }
                };
            }
        }
    }
}