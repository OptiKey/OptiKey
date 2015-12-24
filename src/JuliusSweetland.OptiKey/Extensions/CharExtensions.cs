using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class CharExtensions
    {
        public static char ToUpperAndRemoveDiacritics(this char c)
        {
            //Convert char to string (for the next operations)
            var cAsString = c.ToString(CultureInfo.InvariantCulture);

            //Remove diacritics
            cAsString.RemoveDiacritics();

            //Make uppercase
            cAsString = cAsString.ToUpper();

            //Convert back to char
            return cAsString.First();
        }

        public static CharCategories ToCharCategory(this char c)
        {
            if (new[] { '\n' }.Contains(c))
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
            return input.ToString(CultureInfo.InvariantCulture)
                .Replace("\0", @"\0")
                .Replace("\a", @"\a")
                .Replace("\b", @"\b")
                .Replace("\t", @"\t")
                .Replace("\f", @"\f")
                .Replace("\n", @"\n")
                .Replace("\r", @"\r");
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
