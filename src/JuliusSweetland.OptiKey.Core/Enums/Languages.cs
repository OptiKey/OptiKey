// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
using System.Globalization;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum Languages
    {
        CatalanSpain,
        CroatianCroatia,
        CzechCzechRepublic,
        DanishDenmark,
        DutchBelgium,
        DutchNetherlands,
        EnglishCanada,
        EnglishUK,
        EnglishUS,
        FinnishFinland,
        FrenchCanada,
        FrenchFrance,
        GeorgianGeorgia,
        GermanGermany,
        GreekGreece,
        HebrewIsrael,
        HindiIndia,
        HungarianHungary,
        ItalianItaly,
        JapaneseJapan,
        KoreanKorea,
        PersianIran,
        PolishPoland,
        PortuguesePortugal,
        PortugueseBrazil,
        RussianRussia,
        SerbianSerbia,
        SlovenianSlovenia,
        SlovakSlovakia,
        SpanishSpain,
        TurkishTurkey,
        UkrainianUkraine,
        UrduPakistan
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this Languages language)
        {
            switch (language)
            {
                case Languages.CatalanSpain: return Resources.CATALAN_SPAIN;
                case Languages.CroatianCroatia: return Resources.CROATIAN_CROATIA;
                case Languages.CzechCzechRepublic: return Resources.CZECH_CZECH_REPUBLIC;
                case Languages.DanishDenmark: return Resources.DANISH_DENMARK;
                case Languages.DutchBelgium: return Resources.DUTCH_BELGIUM;
                case Languages.DutchNetherlands: return Resources.DUTCH_NETHERLANDS;
                case Languages.EnglishCanada: return Resources.ENGLISH_CANADA;
                case Languages.EnglishUK: return Resources.ENGLISH_UK;
                case Languages.EnglishUS: return Resources.ENGLISH_US;
                case Languages.FinnishFinland: return Resources.FINNISH_FINLAND;
                case Languages.FrenchCanada: return Resources.FRENCH_CANADA;
                case Languages.FrenchFrance: return Resources.FRENCH_FRANCE;
                case Languages.GeorgianGeorgia: return Resources.GEORGIAN_GEORGIA;
                case Languages.GermanGermany: return Resources.GERMAN_GERMANY;
                case Languages.GreekGreece: return Resources.GREEK_GREECE;
                case Languages.HebrewIsrael: return Resources.HEBREW_ISRAEL;
                case Languages.HindiIndia: return Resources.HINDI_INDIA;
                case Languages.HungarianHungary: return Resources.HUNGARIAN_HUNGARY;
                case Languages.ItalianItaly: return Resources.ITALIAN_ITALY;
                case Languages.JapaneseJapan: return Resources.JAPANESE_JAPAN;
                case Languages.KoreanKorea: return Resources.KOREAN_KOREA;
                case Languages.PersianIran: return Resources.PERSIAN_IRAN;
                case Languages.PolishPoland: return Resources.POLISH_POLAND;
                case Languages.PortuguesePortugal: return Resources.PORTUGUESE_PORTUGAL;
                case Languages.PortugueseBrazil: return Resources.PORTUGUESE_BRAZILIAN;
                case Languages.RussianRussia: return Resources.RUSSIAN_RUSSIA;
                case Languages.SerbianSerbia: return Resources.SERBIAN_SERBIA;
                case Languages.SlovakSlovakia: return Resources.SLOVAK_SLOVAKIA;
                case Languages.SlovenianSlovenia: return Resources.SLOVENIAN_SLOVENIA;
                case Languages.SpanishSpain: return Resources.SPANISH_SPAIN;
                case Languages.TurkishTurkey: return Resources.TURKISH_TURKEY;
                case Languages.UkrainianUkraine: return Resources.UKRAINIAN_UKRAINE;
                case Languages.UrduPakistan: return Resources.URDU_PAKISTAN;
            }

            return language.ToString();
        }

        public static CultureInfo ToCultureInfo(this Languages language)
        {
            switch (language)
            {
                case Languages.CatalanSpain: return CultureInfo.GetCultureInfo("ca-ES");
                case Languages.CroatianCroatia: return CultureInfo.GetCultureInfo("hr-HR");
                case Languages.CzechCzechRepublic: return CultureInfo.GetCultureInfo("cs-CZ");
                case Languages.DanishDenmark: return CultureInfo.GetCultureInfo("da-DK");
                case Languages.DutchBelgium: return CultureInfo.GetCultureInfo("nl-BE");
                case Languages.DutchNetherlands: return CultureInfo.GetCultureInfo("nl-NL");
                case Languages.EnglishUS: return CultureInfo.GetCultureInfo("en-US");
                case Languages.EnglishUK: return CultureInfo.GetCultureInfo("en-GB");
                case Languages.EnglishCanada: return CultureInfo.GetCultureInfo("en-CA");
                case Languages.FinnishFinland: return CultureInfo.GetCultureInfo("fi-FI");
                case Languages.FrenchCanada: return CultureInfo.GetCultureInfo("fr-CA");
                case Languages.FrenchFrance: return CultureInfo.GetCultureInfo("fr-FR");
                case Languages.GeorgianGeorgia: return CultureInfo.GetCultureInfo("ka-GE");
                case Languages.GermanGermany: return CultureInfo.GetCultureInfo("de-DE");
                case Languages.GreekGreece: return CultureInfo.GetCultureInfo("el-GR");
                case Languages.HebrewIsrael: return CultureInfo.GetCultureInfo("he-IL");
                case Languages.HindiIndia: return CultureInfo.GetCultureInfo("hi-IN");
                case Languages.HungarianHungary: return CultureInfo.GetCultureInfo("hu-HU");
                case Languages.ItalianItaly: return CultureInfo.GetCultureInfo("it-IT");
                case Languages.JapaneseJapan: return CultureInfo.GetCultureInfo("ja-JP");
                case Languages.KoreanKorea: return CultureInfo.GetCultureInfo("ko-KR");
                case Languages.PersianIran: return CultureInfo.GetCultureInfo("fa-IR");
                case Languages.PolishPoland: return CultureInfo.GetCultureInfo("pl-PL");
                case Languages.PortuguesePortugal: return CultureInfo.GetCultureInfo("pt-PT");
                case Languages.PortugueseBrazil: return CultureInfo.GetCultureInfo("pt-BR");
                case Languages.RussianRussia: return CultureInfo.GetCultureInfo("ru-RU");
                case Languages.SerbianSerbia: return CultureInfo.GetCultureInfo("sr-Cyrl-RS");
                case Languages.SlovakSlovakia: return CultureInfo.GetCultureInfo("sk-SK");
                case Languages.SlovenianSlovenia: return CultureInfo.GetCultureInfo("sl-SI");
                case Languages.SpanishSpain: return CultureInfo.GetCultureInfo("es-ES");
                case Languages.TurkishTurkey: return CultureInfo.GetCultureInfo("tr-TR");
                case Languages.UkrainianUkraine: return CultureInfo.GetCultureInfo("uk-UA");
                case Languages.UrduPakistan: return CultureInfo.GetCultureInfo("ur-PK");
            }
            return CultureInfo.GetCultureInfo("en-GB");
        }

        public static bool SupportsAutoSpace(this Languages language)
        {
            switch (language)
            {
                case Languages.JapaneseJapan:
                case Languages.KoreanKorea:
                    return false;

                default:
                    return true;
            }
        }
    }
}
