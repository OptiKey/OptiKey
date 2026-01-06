// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class CharExtensionsTests
    {
        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }

        [Test]
        public void TestToCharCategory()
        {
            Assert.Multiple(() =>
            {
                Assert.That('\n'.ToCharCategory(), Is.EqualTo(CharCategories.NewLine));
                Assert.That(' '.ToCharCategory(), Is.EqualTo(CharCategories.Space));
                Assert.That('\t'.ToCharCategory(), Is.EqualTo(CharCategories.Tab));
                Assert.That('s'.ToCharCategory(), Is.EqualTo(CharCategories.WordCharacter));
                Assert.That('5'.ToCharCategory(), Is.EqualTo(CharCategories.WordCharacter));
                Assert.That('!'.ToCharCategory(), Is.EqualTo(CharCategories.OtherSymbol));
                Assert.That('\a'.ToCharCategory(), Is.EqualTo(CharCategories.SomethingElse));
                Assert.That('\r'.ToCharCategory(), Is.EqualTo(CharCategories.SomethingElse));
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
            });            
        }

        [Test]
        public void TestToVirtualKeyCode()
        {
            Assert.Multiple(() =>
            {
                Assert.That(' '.ToVirtualKeyCode(), Is.EqualTo(VirtualKeyCode.SPACE));
                Assert.That('\t'.ToVirtualKeyCode(), Is.EqualTo(VirtualKeyCode.TAB));
                Assert.That('\n'.ToVirtualKeyCode(), Is.EqualTo(VirtualKeyCode.RETURN));
                Assert.That('Z'.ToVirtualKeyCode(), Is.Null);
            });           

        }

    }
}
