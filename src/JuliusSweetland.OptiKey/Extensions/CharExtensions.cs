using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class CharExtensions
    {
        public static CharCategories ToCharCategory(this char c)
        {
            if (c == '\n')
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

        public static string ConvertEscapedCharToLiteral(this char input)
        {
            return input.ToString(CultureInfo.InvariantCulture).ConvertEscapedCharsToLiterals();
        }

        public static bool IsCombiningCharacter(this char c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            return category == UnicodeCategory.NonSpacingMark //(All combining diacritic characters are non-spacing marks). Nonspacing character that indicates modifications of a base character. Signified by the Unicode designation "Mn"(mark, nonspacing).The value is 5.
                || category == UnicodeCategory.SpacingCombiningMark //Spacing character that indicates modifications of a base character and affects the width of the glyph for that base character. Signified by the Unicode designation "Mc" (mark, spacing combining). The value is 6.
                || category == UnicodeCategory.EnclosingMark; //Enclosing mark character, which is a nonspacing combining character that surrounds all previous characters up to and including a base character. Signified by the Unicode designation "Me" (mark, enclosing). The value is 7.
        }

        //http://inputsimulator.codeplex.com/SourceControl/latest#WindowsInput/Native/VirtualKeyCode.cs
        //http://msdn.microsoft.com/en-gb/library/windows/desktop/dd375731(v=vs.85).aspx
        public static VirtualKeyCode? ToVirtualKeyCode(this char character)
        {
            switch (character)
            {
                case ' ':
                    return VirtualKeyCode.SPACE;

                case '\t':
                    return VirtualKeyCode.TAB;

                case '\n':
                    return VirtualKeyCode.RETURN;

                default:
                    return null;
            }
        }
    }
}
