using System.Globalization;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum Languages
    {
        AmericanEnglish,
        BritishEnglish,
        CanadianEnglish, 
        FranceFrench
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this Languages languages)
        {
            switch (languages)
            {
                case Languages.AmericanEnglish: return Resources.AMERICAN_ENGLISH;
                case Languages.BritishEnglish: return Resources.BRITISH_ENGLISH;
                case Languages.CanadianEnglish: return Resources.CANADIAN_ENGLISH;
                case Languages.FranceFrench: return Resources.FRANCE_FRENCH;
            }

            return languages.ToString();
        }

        public static CultureInfo ToCultureInfo(this Languages languages)
        {
            switch (languages)
            {
                case Languages.AmericanEnglish: return CultureInfo.GetCultureInfo("en-US");
                case Languages.BritishEnglish: return CultureInfo.GetCultureInfo("en-GB");
                case Languages.CanadianEnglish: return CultureInfo.GetCultureInfo("en-CA");
                case Languages.FranceFrench: return CultureInfo.GetCultureInfo("fr-FR");
            }

            return CultureInfo.GetCultureInfo("en-GB");
        }
    }
}
