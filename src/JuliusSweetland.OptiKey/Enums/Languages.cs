using System.Globalization;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum Languages
    {
        CroatianCroatia,
        DanishDenmark,
        DutchBelgium,
        DutchNetherlands,
        EnglishCanada,
        EnglishUK,
        EnglishUS,
	    FrenchFrance,
        GermanGermany,
        GreekGreece,
        ItalianItaly,
        PortuguesePortugal,
        RussianRussia,
        SlovenianSlovenia,
        SpanishSpain,
        TurkishTurkey
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this Languages languages)
        {
            switch (languages)
            {
                case Languages.CroatianCroatia: return Resources.CROATIAN_CROATIA;
                case Languages.DanishDenmark: return Resources.DANISH_DENMARK;
                case Languages.DutchBelgium: return Resources.DUTCH_BELGIUM;
                case Languages.DutchNetherlands: return Resources.DUTCH_NETHERLANDS;
                case Languages.EnglishCanada: return Resources.ENGLISH_CANADA;
                case Languages.EnglishUK: return Resources.ENGLISH_UK;
                case Languages.EnglishUS: return Resources.ENGLISH_US;
                case Languages.FrenchFrance: return Resources.FRENCH_FRANCE;
                case Languages.GermanGermany: return Resources.GERMAN_GERMANY;
                case Languages.GreekGreece: return Resources.GREEK_GREECE;
                case Languages.ItalianItaly: return Resources.ITALIAN_ITALY;
                case Languages.PortuguesePortugal: return Resources.PORTUGUESE_PORTUGAL;
                case Languages.RussianRussia: return Resources.RUSSIAN_RUSSIA;
                case Languages.SlovenianSlovenia: return Resources.SLOVENIAN_SLOVENIA;
                case Languages.SpanishSpain: return Resources.SPANISH_SPAIN;
                case Languages.TurkishTurkey: return Resources.TURKISH_TURKEY;
            }

            return languages.ToString();
        }

        public static CultureInfo ToCultureInfo(this Languages languages)
        {
            switch (languages)
            {
                case Languages.CroatianCroatia: return CultureInfo.GetCultureInfo("hr-HR");
                case Languages.DanishDenmark: return CultureInfo.GetCultureInfo("da-DK");
                case Languages.DutchBelgium: return CultureInfo.GetCultureInfo("nl-BE");
                case Languages.DutchNetherlands: return CultureInfo.GetCultureInfo("nl-NL");			
                case Languages.EnglishUS: return CultureInfo.GetCultureInfo("en-US");
                case Languages.EnglishUK: return CultureInfo.GetCultureInfo("en-GB");
                case Languages.EnglishCanada: return CultureInfo.GetCultureInfo("en-CA");
                case Languages.FrenchFrance: return CultureInfo.GetCultureInfo("fr-FR");
                case Languages.GermanGermany: return CultureInfo.GetCultureInfo("de-DE");
                case Languages.GreekGreece: return CultureInfo.GetCultureInfo("el-GR");
                case Languages.ItalianItaly: return CultureInfo.GetCultureInfo("it-IT");
                case Languages.PortuguesePortugal: return CultureInfo.GetCultureInfo("pt-PT");
                case Languages.RussianRussia: return CultureInfo.GetCultureInfo("ru-RU");
                case Languages.SlovenianSlovenia: return CultureInfo.GetCultureInfo("sl-SI");
                case Languages.SpanishSpain: return CultureInfo.GetCultureInfo("es-ES");
                case Languages.TurkishTurkey: return CultureInfo.GetCultureInfo("tr-TR");
            }
            return CultureInfo.GetCultureInfo("en-GB");
        }
    }
}
