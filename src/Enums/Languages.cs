namespace JuliusSweetland.OptiKey.Enums
{
    public enum Languages
    {
        AmericanEnglish,
        BritishEnglish,
        CanadianEnglish
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this Languages languages)
        {
            switch (languages)
            {
                case Languages.AmericanEnglish: return "American English";
                case Languages.BritishEnglish: return "British English";
                case Languages.CanadianEnglish: return "Canadian English";
            }

            return languages.ToString();
        }
    }
}
