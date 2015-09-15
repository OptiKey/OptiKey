namespace JuliusSweetland.OptiKey.Enums
{
    public enum Languages
    {
        AmericanEnglish,
        BritishEnglish,
        CanadianEnglish,
        Russian,
        German,
        Lithuanian
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this Languages languages)
        {
            switch (languages)
            {
                case Languages.AmericanEnglish: return "en-US";
                case Languages.BritishEnglish: return "en-GB";
                case Languages.CanadianEnglish: return "en-CA";
                case Languages.Russian: return "ru";
                case Languages.German: return "de";
                case Languages.Lithuanian: return "lt";
            }

            return languages.ToString();
        }

        public static string ToFullDescription(this Languages languages)
        {
            switch (languages)
            {
                case Languages.AmericanEnglish: return "American English";
                case Languages.BritishEnglish: return "British English";
                case Languages.CanadianEnglish: return "Canadian English";
                case Languages.Russian: return "Русский";
                case Languages.German: return "Deutsch";
                case Languages.Lithuanian: return "Lietuvių";
            }

            return languages.ToString();
        }
    }
}
