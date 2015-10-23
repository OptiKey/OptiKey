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
                case 'a':
                case 'A':
                case 'ф':
                case 'Ф':
                    return VirtualKeyCode.VK_A;

                case 'b':
                case 'B':
                case 'и':
                case 'И':
                    return VirtualKeyCode.VK_B;

                case 'c':
                case 'C':
                case 'с':
                case 'С':
                    return VirtualKeyCode.VK_C;

                case 'd':
                case 'D':
                case 'б':
                case 'Б':
                    return VirtualKeyCode.VK_D;

                case 'e':
                case 'E':
                case 'у':
                case 'У':
                    return VirtualKeyCode.VK_E;

                case 'f':
                case 'F':
                case 'а':
                case 'А':
                    return VirtualKeyCode.VK_F;

                case 'g':
                case 'G':
                case 'п':
                case 'П':
                    return VirtualKeyCode.VK_G;

                case 'h':
                case 'H':
                case 'р':
                case 'Р':
                    return VirtualKeyCode.VK_H;

                case 'i':
                case 'I':
                case 'ш':
                case 'Ш':
                    return VirtualKeyCode.VK_I;

                case 'j':
                case 'J':
                case 'о':
                case 'О':
                    return VirtualKeyCode.VK_J;

                case 'k':
                case 'K':
                case 'л':
                case 'Л':
                    return VirtualKeyCode.VK_K;

                case 'l':
                case 'L':
                case 'д':
                case 'Д':
                    return VirtualKeyCode.VK_L;

                case 'm':
                case 'M':
                case 'ь':
                case 'Ь':
                    return VirtualKeyCode.VK_M;

                case 'n':
                case 'N':
                case 'т':
                case 'Т':
                    return VirtualKeyCode.VK_N;

                case 'o':
                case 'O':
                case 'щ':
                case 'Щ':
                    return VirtualKeyCode.VK_O;

                case 'p':
                case 'P':
                case 'з':
                case 'З':
                    return VirtualKeyCode.VK_P;

                case 'q':
                case 'Q':
                case 'й':
                case 'Й':
                    return VirtualKeyCode.VK_Q;

                case 'r':
                case 'R':
                case 'к':
                case 'К':
                    return VirtualKeyCode.VK_R;

                case 's':
                case 'S':
                case 'ы':
                case 'Ы':
                    return VirtualKeyCode.VK_S;

                case 't':
                case 'T':
                case 'е':
                case 'Е':
                    return VirtualKeyCode.VK_T;

                case 'u':
                case 'U':
                case 'г':
                case 'Г':
                    return VirtualKeyCode.VK_U;

                case 'v':
                case 'V':
                case 'м':
                case 'М':
                    return VirtualKeyCode.VK_V;

                case 'w':
                case 'W':
                case 'ц':
                case 'Ц':
                    return VirtualKeyCode.VK_W;

                case 'x':
                case 'X':
                case 'ч':
                case 'Ч':
                    return VirtualKeyCode.VK_X;

                case 'y':
                case 'Y':
                case 'н':
                case 'Н':
                    return VirtualKeyCode.VK_Y;

                case 'z':
                case 'Z':
                case 'я':
                case 'Я':
                    return VirtualKeyCode.VK_Z;

                case '1':
                    return VirtualKeyCode.VK_1;

                case '2':
                    return VirtualKeyCode.VK_2;

                case '3':
                    return VirtualKeyCode.VK_3;

                case '4':
                    return VirtualKeyCode.VK_4;

                case '5':
                    return VirtualKeyCode.VK_5;

                case '6':
                    return VirtualKeyCode.VK_6;

                case '7':
                    return VirtualKeyCode.VK_7;

                case '8':
                    return VirtualKeyCode.VK_8;

                case '9':
                    return VirtualKeyCode.VK_9;

                case '0':
                    return VirtualKeyCode.VK_0;

                case ' ':
                    return VirtualKeyCode.SPACE ;

                case '\t':
                    return VirtualKeyCode.TAB ;

                case '\n':
                    return VirtualKeyCode.RETURN ;

                default:
                    return null;
            }
        }
    }
}
