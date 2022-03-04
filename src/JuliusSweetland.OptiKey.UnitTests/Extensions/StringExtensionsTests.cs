// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Extensions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        private const string NullString = null;

        [Test]
        public void TestFirstCharToUpper()
        {
            Assert.Multiple(() =>
            {
                Assert.That("optikey".FirstCharToUpper(), Is.EqualTo("Optikey"));
                Assert.That("Optikey".FirstCharToUpper(), Is.EqualTo("Optikey"));
                Assert.That("OptiKey".FirstCharToUpper(), Is.EqualTo("OptiKey"));
                Assert.That("sam Was Here".FirstCharToUpper(), Is.EqualTo("Sam Was Here"));
                Assert.That("%$%@$#%#@#$&^%$&#$".FirstCharToUpper(), Is.EqualTo("%$%@$#%#@#$&^%$&#$"));
                Assert.That("  ".FirstCharToUpper(), Is.EqualTo("  "));
                Assert.That(NullString.FirstCharToUpper(), Is.Null);
            });            
        }

        [Test]
        public void TestConvertEscapedCharToLiteral()
        {
            Assert.Multiple(() =>
            {
                Assert.That('\0'.ToPrintableString(), Is.EqualTo(@"[Char:\0|Unicode:U+0000]"));
                Assert.That('\a'.ToPrintableString(), Is.EqualTo(@"[Char:\a|Unicode:U+0007]"));
                Assert.That('\b'.ToPrintableString(), Is.EqualTo(@"[Char:\b|Unicode:U+0008]"));
                Assert.That('\t'.ToPrintableString(), Is.EqualTo(@"[Char:\t|Unicode:U+0009]"));
                Assert.That('\f'.ToPrintableString(), Is.EqualTo(@"[Char:\f|Unicode:U+000c]"));
                Assert.That('\n'.ToPrintableString(), Is.EqualTo(@"[Char:\n|Unicode:U+000a]"));
                Assert.That('\r'.ToPrintableString(), Is.EqualTo(@"[Char:\r|Unicode:U+000d]"));
                Assert.That('s'.ToPrintableString(), Is.EqualTo(@"[Char:s|Unicode:U+0073]"));
                Assert.That(' '.ToPrintableString(), Is.EqualTo(@"[Char: |Unicode:U+0020]"));
                Assert.That(NullString.ToPrintableString(), Is.Null);
            });            
        }
        
        [Test]
        public void TestNextCharacterWouldBeStartOfNewSentence()
        {
            Assert.Multiple(() =>
            {
                Assert.That(NullString.NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("".NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("This is a sentence. ".NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("This is a sentence! ".NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("This is a sentence? ".NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("This is a sentence\n".NextCharacterWouldBeStartOfNewSentence(), Is.True);
                Assert.That("This is a sentence where i forgot my punctuation ".NextCharacterWouldBeStartOfNewSentence(), Is.False);
                Assert.That("This is a sentence.".NextCharacterWouldBeStartOfNewSentence(), Is.False); //Does not have trailing space
            });           

        }
        
        [Test]
        public void TestInProgressWord()
        {
            var sentence = "This is a sentence.";

            Assert.Multiple(() =>
            {
                Assert.That(sentence.InProgressWord(0), Is.Null);
                Assert.That(sentence.InProgressWord(1), Is.EqualTo("This"));
                Assert.That(sentence.InProgressWord(2), Is.EqualTo("This"));
                Assert.That(sentence.InProgressWord(3), Is.EqualTo("This"));
                Assert.That(sentence.InProgressWord(4), Is.EqualTo("This"));
                Assert.That(sentence.InProgressWord(5), Is.Null);
                Assert.That(sentence.InProgressWord(6), Is.EqualTo("is"));
                Assert.That(sentence.InProgressWord(7), Is.EqualTo("is"));
                Assert.That(sentence.InProgressWord(8), Is.Null);
                Assert.That(sentence.InProgressWord(9), Is.EqualTo("a"));
                Assert.That(sentence.InProgressWord(10), Is.Null);
                Assert.That(sentence.InProgressWord(11), Is.EqualTo("sentence."));
            });            
        }

        [Test]
        public void TestRemoveDiacritics()
        {
            Assert.Multiple(() =>
            {
                Assert.That("aÀå".RemoveDiacritics(), Is.EqualTo("aAa"));
                Assert.That("ČĔĨŞē".RemoveDiacritics(), Is.EqualTo("CEISe"));
                Assert.That("ČĔĨŞē".RemoveDiacritics(false), Is.EqualTo("CEISe"));
            });            
           
        }

        [Test]
        public void TestLongestCommonSubsequence()
        {
            Assert.Multiple(() =>
            {
                Assert.That("This is a sentence.".LongestCommonSubsequence("This is another sentence."), Is.EqualTo(19));
                Assert.That("cat hat".LongestCommonSubsequence("cat in the hat"), Is.EqualTo(7));
                Assert.That("cat".LongestCommonSubsequence("cat in the hat"), Is.EqualTo(3));
                Assert.That("CAT".LongestCommonSubsequence("cat in the hat"), Is.EqualTo(0));
            });
        }

        [Test]
        public void TestToString()
        {
            var stringCollection = new List<string>
            {
                 "This",
                 "is",
                 "a",
                 "comma",
                 "seperated",
                 "list",

            };

            var emptyStringCollection = new List<string>();
            List<string> nullStringCollection = null;

            Assert.Multiple(() =>
            {
                Assert.That(stringCollection.ToString(""), Is.EqualTo("This,is,a,comma,seperated,list"));
                Assert.That(stringCollection.ToString("Base string: "), Is.EqualTo("This,is,a,comma,seperated,list"));
                Assert.That(emptyStringCollection.ToString("EMPTY"), Is.EqualTo(""));
                // ReSharper disable once ExpressionIsAlwaysNull
                Assert.That(nullStringCollection.ToString("EMPTY"), Is.EqualTo("EMPTY"));
            });            
        }

        [Test]
        public void TestExtraWordsAndLines()
        {
            List<string> finalList = new List<string>
            {
                "This",
                "is",
                "sentence",
                "This is a sentence"
            };

            Assert.Multiple(() =>
            {
                Assert.That("  ".ExtractWordsAndLines(), Is.Null);
                Assert.That(NullString.ExtractWordsAndLines(), Is.Null);
            });            

            var list = "This is a sentence".ExtractWordsAndLines();
            Assert.That(list.Count, Is.EqualTo(4));
            Assert.That(list, Is.EqualTo(finalList));

            var list2 = "This is a sentence\nThis is a sentence".ExtractWordsAndLines();
            Assert.That(list2.Count, Is.EqualTo(4));
            Assert.That(list2, Is.EqualTo(finalList));
        }

        [Test]
        public void TestCreateDictionaryEntryHash()
        {
            Assert.Multiple(() =>
            {
                Assert.That("  ".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.Null);
                Assert.That(NullString.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.Null);
                Assert.That("EntryWithSpaceAtEnd   ".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.EqualTo("ENTRYWITHSPACEATEND"));
                Assert.That("ČĔĨŞē".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.EqualTo("CEISE"));
                Assert.That("This has spaces".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.EqualTo("THS"));
                Assert.That("This has spaces".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(false), Is.EqualTo("THS"));
                Assert.That("5Cats 6dogs 7Goats".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.Null);
                Assert.That("Optikey is awesome. I want 2 copies".NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(), Is.EqualTo("OIAIWC"));
            });          
        }
        
        [Test]
        public void TestCreateAutoCompleteDictionaryEntryHash()
        {
            Assert.Multiple(() =>
            {
                Assert.That("  ".Normalise(), Is.Null);
                Assert.That(NullString.Normalise(), Is.Null);
                Assert.That("EntryWithSpaceAtEnd   ".Normalise(), Is.EqualTo("ENTRYWITHSPACEATEND"));
                Assert.That("ČĔĨŞē".Normalise(), Is.EqualTo("CEISE"));
                Assert.That("This has spaces".Normalise(), Is.EqualTo("THIS HAS SPACES"));
                Assert.That("This has spaces".Normalise(false), Is.EqualTo("THIS HAS SPACES"));
                Assert.That("5Cats 6dogs 7Goats".Normalise(), Is.EqualTo("5CATS 6DOGS 7GOATS"));
                Assert.That("Optikey is awesome. I want 2 copies".Normalise(), Is.EqualTo("OPTIKEY IS AWESOME. I WANT 2 COPIES"));
            });            
        }

        [Test]
        public void TestCleanupPossibleDictionaryEntry()
        {
            Assert.Multiple(() =>
            {
                Assert.That("  ".CleanupPossibleDictionaryEntry(), Is.Null);
                Assert.That(NullString.CleanupPossibleDictionaryEntry(), Is.Null);
                Assert.That("Word!".CleanupPossibleDictionaryEntry(), Is.EqualTo("Word"));
                Assert.That("Cat".CleanupPossibleDictionaryEntry(), Is.EqualTo("Cat"));
                Assert.That("This is a sentence".CleanupPossibleDictionaryEntry(), Is.EqualTo("This is a sentence"));
                Assert.That("Čats!".CleanupPossibleDictionaryEntry(), Is.EqualTo("Čats"));
            });            
        }

        [Test]
        public void TestToCharListWithCounts()
        {
            var strings = new List<string>
            {
                "Čats",
                "Word!",
                "THIS HAS SPACES",
                "OPTIKEY IS AWESOME. I WANT 2 COPIES"
            };

          var charListWithCounts = strings.ToCharListWithCounts();
          Assert.That(4, Is.EqualTo(charListWithCounts.Count));
          AssertCharTuple(charListWithCounts[0], 'C', 'Č', 1);
          AssertCharTuple(charListWithCounts[1], 'W', 'W', 1);
          AssertCharTuple(charListWithCounts[2], 'T', 'T', 1);
          AssertCharTuple(charListWithCounts[3], 'O', 'O', 1);

        }

        private void AssertCharTuple(Tuple<char,char,int> tuple, char expectedItem1, char expectedItem2, int expectedItem3)
        {
            Assert.Multiple(() =>
            {
                Assert.That(tuple.Item1, Is.EqualTo(expectedItem1));
                Assert.That(tuple.Item2, Is.EqualTo(expectedItem2));
                Assert.That(tuple.Item3, Is.EqualTo(expectedItem3));
            });            
        }
    }
}
