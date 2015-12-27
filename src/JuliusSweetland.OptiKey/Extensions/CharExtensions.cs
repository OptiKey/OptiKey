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
