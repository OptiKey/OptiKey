using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class CharExtensionsTests
    {
        [Test]
        public void TestToCharCategory()
        {
            Assert.AreEqual(CharCategories.NewLine, '\n'.ToCharCategory());
            Assert.AreEqual(CharCategories.Space, ' '.ToCharCategory());
            Assert.AreEqual(CharCategories.Tab, '\t'.ToCharCategory());
            Assert.AreEqual(CharCategories.LetterOrDigitOrSymbolOrPunctuation, 's'.ToCharCategory());
            Assert.AreEqual(CharCategories.LetterOrDigitOrSymbolOrPunctuation, '5'.ToCharCategory());
            Assert.AreEqual(CharCategories.LetterOrDigitOrSymbolOrPunctuation, '!'.ToCharCategory());
            Assert.AreEqual(CharCategories.SomethingElse, '\a'.ToCharCategory());
            Assert.AreEqual(CharCategories.SomethingElse, '\r'.ToCharCategory());
        }

        [Test]
        public void TestConvertEscapedCharToLiteral()
        {
            Assert.AreEqual(@"\0", '\0'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\a", '\a'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\b", '\b'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\t", '\t'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\f", '\f'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\n", '\n'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"\r", '\r'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@"s", 's'.ConvertEscapedCharToLiteral());
            Assert.AreEqual(@" ", ' '.ConvertEscapedCharToLiteral());
        }

        [Test]
        public void TestToVirtualKeyCode()
        {
            Assert.AreEqual(VirtualKeyCode.SPACE, ' '.ToVirtualKeyCode());
            Assert.AreEqual(VirtualKeyCode.TAB, '\t'.ToVirtualKeyCode());
            Assert.AreEqual(VirtualKeyCode.RETURN, '\n'.ToVirtualKeyCode());
            Assert.IsNull('Z'.ToVirtualKeyCode());

        }

    }
}
