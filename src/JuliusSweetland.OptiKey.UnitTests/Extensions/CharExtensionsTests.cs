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
            Assert.AreEqual(@"\0", '\0'.ToPrintableString());
            Assert.AreEqual(@"\a", '\a'.ToPrintableString());
            Assert.AreEqual(@"\b", '\b'.ToPrintableString());
            Assert.AreEqual(@"\t", '\t'.ToPrintableString());
            Assert.AreEqual(@"\f", '\f'.ToPrintableString());
            Assert.AreEqual(@"\n", '\n'.ToPrintableString());
            Assert.AreEqual(@"\r", '\r'.ToPrintableString());
            Assert.AreEqual(@"s", 's'.ToPrintableString());
            Assert.AreEqual(@" ", ' '.ToPrintableString());
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
