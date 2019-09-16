// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using WindowsInput.Native;
using JuliusSweetland.OptiKey.InstallerActions;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests
{
    [TestFixture]
    public class InstallerTests
    {
        [Test]
        public void TestSupportedLanguageOnly()
        {
            string langAndCountry = "German";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            Assert.True(match.Contains("German"));
        }

        [Test]
        public void TestDefault()
        {
            string langAndCountry = "";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            // If not supported at all, default to uk english
            Assert.True(match.Contains("English"));
            Assert.True(match.Contains("UK"));
        }

        [Test]
        public void TestUnsupportedLanguageOnly()
        {
            string langAndCountry = "Elvish";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            // If not supported at all, default to uk english
            Assert.True(match.Contains("English"));
            Assert.True(match.Contains("UK"));

        }

        [Test]
        public void TestSupportedLanguageAndCountry()
        {
            string langAndCountry = "Danish (Denmark)";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            Assert.True(match.Contains("Danish"));
            Assert.True(match.Contains("Denmark"));
        }


        [Test]
        public void TestSupportedLanguageAndCountryMultiple()
        {
            string langAndCountry = "English (US, Australia)";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            Assert.True(match.Contains("English"));
            Assert.True(match.Contains("US"));
        }

        [Test]
        public void TestSupportedLanguageAndUnsupportedCountry()
        {
            string langAndCountry = "Danish (Greenland)";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            // Default to matching language, if not country
            Assert.True(match.Contains("Danish"));
        }

        [Test]
        public void TestSupportedLanguageAndUnsupportedCountryWithDefaults()
        {
            string langAndCountry = "English (Australia)";
            string match = CustomActions.GetDefaultLanguage(langAndCountry);

            // Default to English UK 
            Assert.True(match.Contains("English"));
            Assert.True(match.Contains("UK"));
        }

    }
}
