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
            Assert.AreEqual("Optikey", "optikey".FirstCharToUpper());
            Assert.AreEqual("Optikey", "Optikey".FirstCharToUpper());
            Assert.AreEqual("OptiKey", "OptiKey".FirstCharToUpper());
            Assert.AreEqual("Sam Was Here", "sam Was Here".FirstCharToUpper());
            Assert.AreEqual("%$%@$#%#@#$&^%$&#$", "%$%@$#%#@#$&^%$&#$".FirstCharToUpper());
            Assert.AreEqual("  ", "  ".FirstCharToUpper());
            Assert.AreEqual(null, NullString.FirstCharToUpper());
        }

        [Test]
        public void TestConvertEscapedCharToLiteral()
        {
            Assert.AreEqual(@"\0", "\0".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\a", "\a".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\b", "\b".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\t", "\t".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\f", "\f".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\n", "\n".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"\r", "\r".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@"s", "s".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(@" ", " ".ConvertEscapedCharsToLiterals());
            Assert.AreEqual(null, NullString.ConvertEscapedCharsToLiterals());
        }
        
        [Test]
        public void TestNextCharacterWouldBeStartOfNewSentence()
        {
            Assert.AreEqual(true, NullString.NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(true, "".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(true, "This is a sentence. ".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(true, "This is a sentence! ".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(true, "This is a sentence? ".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(true, "This is a sentence\n".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(false, "This is a sentence where i forgot my punctuation ".NextCharacterWouldBeStartOfNewSentence());
            Assert.AreEqual(false, "This is a sentence.".NextCharacterWouldBeStartOfNewSentence()); //Does not have trailing space

        }
        
        [Test]
        public void TestInProgressWord()
        {
            var sentence = "This is a sentence.";
            Assert.AreEqual(null, sentence.InProgressWord(0));
            Assert.AreEqual("This", sentence.InProgressWord(1));
            Assert.AreEqual("This", sentence.InProgressWord(2));
            Assert.AreEqual("This", sentence.InProgressWord(3));
            Assert.AreEqual("This", sentence.InProgressWord(4));
            Assert.AreEqual(null, sentence.InProgressWord(5));
            Assert.AreEqual("is", sentence.InProgressWord(6));
            Assert.AreEqual("is", sentence.InProgressWord(7));
            Assert.AreEqual(null, sentence.InProgressWord(8));
            Assert.AreEqual("a", sentence.InProgressWord(9));
            Assert.AreEqual(null, sentence.InProgressWord(10));
            Assert.AreEqual("sentence.", sentence.InProgressWord(11));
        }

        [Test]
        public void TestRemoveDiacritics()
        {
            Assert.AreEqual("aAa", "aÀå".RemoveDiacritics());
            Assert.AreEqual("CEISe", "ČĔĨŞē".RemoveDiacritics());
            Assert.AreEqual("CEISe", "ČĔĨŞē".RemoveDiacritics(false));
           
        }

        [Test]
        public void TestLongestCommonSubsequence()
        {
            Assert.AreEqual(19, "This is a sentence.".LongestCommonSubsequence("This is another sentence."));
            Assert.AreEqual(7, "cat hat".LongestCommonSubsequence("cat in the hat"));
            Assert.AreEqual(3, "cat".LongestCommonSubsequence("cat in the hat"));
            Assert.AreEqual(0, "CAT".LongestCommonSubsequence("cat in the hat"));
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
            Assert.AreEqual("This,is,a,comma,seperated,list", stringCollection.ToString(""));
            Assert.AreEqual("This,is,a,comma,seperated,list", stringCollection.ToString("Base string: "));
            Assert.AreEqual("", emptyStringCollection.ToString("EMPTY"));
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual("EMPTY", nullStringCollection.ToString("EMPTY"));
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

            Assert.AreEqual(null, "  ".ExtractWordsAndLines());
            Assert.AreEqual(null, NullString.ExtractWordsAndLines());

            var list = "This is a sentence".ExtractWordsAndLines();
            Assert.AreEqual(4, list.Count);
            CollectionAssert.AreEqual(finalList, list);

            var list2 = "This is a sentence\nThis is a sentence".ExtractWordsAndLines();
            Assert.AreEqual(4, list2.Count);
            CollectionAssert.AreEqual(finalList, list2);
        }

    }
}
