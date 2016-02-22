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
            Assert.AreEqual(@"[Char:\0|Unicode:U+0000]", '\0'.ToPrintableString());
            Assert.AreEqual(@"[Char:\a|Unicode:U+0007]", '\a'.ToPrintableString());
            Assert.AreEqual(@"[Char:\b|Unicode:U+0008]", '\b'.ToPrintableString());
            Assert.AreEqual(@"[Char:\t|Unicode:U+0009]", '\t'.ToPrintableString());
            Assert.AreEqual(@"[Char:\f|Unicode:U+000c]", '\f'.ToPrintableString());
            Assert.AreEqual(@"[Char:\n|Unicode:U+000a]", '\n'.ToPrintableString());
            Assert.AreEqual(@"[Char:\r|Unicode:U+000d]", '\r'.ToPrintableString());
            Assert.AreEqual(@"[Char:s|Unicode:U+0073]", 's'.ToPrintableString());
            Assert.AreEqual(@"[Char: |Unicode:U+0020]", ' '.ToPrintableString());
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
