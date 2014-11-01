using System.Linq;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class CharExtensions
    {
        public static char ToUpperAndRemoveDiacritics(this char c)
        {
            //Convert char to string (for the next operations)
            var cAsString = c.ToString();

            //Remove diacritics
            cAsString.RemoveDiacritics();

            //Make uppercase
            cAsString = cAsString.ToUpper();

            //Convert back to char
            return cAsString.First();
        }

        public static CharCategories ToCharCategory(this char c)
        {
            if (new[] { '\n', '\r' }.Contains(c))
            {
                return CharCategories.NewLine;
            }

            if (c == ' ')
            {
                return CharCategories.Space;
            }

            if (c == '\t')
            {
                return CharCategories.Tab;
            }

            if (char.IsLetterOrDigit(c) || char.IsSymbol(c) || char.IsPunctuation(c))
            {
                return CharCategories.LetterOrDigitOrSymbolOrPunctuation;
            }

            return CharCategories.SomethingElse;
        }
    }
}
