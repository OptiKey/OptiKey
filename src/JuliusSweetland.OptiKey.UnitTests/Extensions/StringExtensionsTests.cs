using System.Collections.Generic;
using System.Windows;
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

    }
}
