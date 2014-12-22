using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Extensions
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
                    return VirtualKeyCode.VK_A;

                case 'b':
                case 'B':
                    return VirtualKeyCode.VK_B;

                case 'c':
                case 'C':
                    return VirtualKeyCode.VK_C;

                case 'd':
                case 'D':
                    return VirtualKeyCode.VK_D;

                case 'e':
                case 'E':
                    return VirtualKeyCode.VK_E;

                case 'f':
                case 'F':
                    return VirtualKeyCode.VK_F;

                case 'g':
                case 'G':
                    return VirtualKeyCode.VK_G;

                case 'h':
                case 'H':
                    return VirtualKeyCode.VK_H;

                case 'i':
                case 'I':
                    return VirtualKeyCode.VK_I;

                case 'j':
                case 'J':
                    return VirtualKeyCode.VK_J;

                case 'k':
                case 'K':
                    return VirtualKeyCode.VK_K;

                case 'l':
                case 'L':
                    return VirtualKeyCode.VK_L;

                case 'm':
                case 'M':
                    return VirtualKeyCode.VK_M;

                case 'n':
                case 'N':
                    return VirtualKeyCode.VK_N;

                case 'o':
                case 'O':
                    return VirtualKeyCode.VK_O;

                case 'p':
                case 'P':
                    return VirtualKeyCode.VK_P;

                case 'q':
                case 'Q':
                    return VirtualKeyCode.VK_Q;

                case 'r':
                case 'R':
                    return VirtualKeyCode.VK_R;

                case 's':
                case 'S':
                    return VirtualKeyCode.VK_S;

                case 't':
                case 'T':
                    return VirtualKeyCode.VK_T;

                case 'u':
                case 'U':
                    return VirtualKeyCode.VK_U;

                case 'v':
                case 'V':
                    return VirtualKeyCode.VK_V;

                case 'w':
                case 'W':
                    return VirtualKeyCode.VK_W;

                case 'x':
                case 'X':
                    return VirtualKeyCode.VK_X;

                case 'y':
                case 'Y':
                    return VirtualKeyCode.VK_Y;

                case 'z':
                case 'Z':
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

                case ',':
                    return VirtualKeyCode.OEM_COMMA ;

                case '.':
                    return VirtualKeyCode.OEM_PERIOD ;

                case '+':
                    return VirtualKeyCode.OEM_PLUS ;

                case '-':
                    return VirtualKeyCode.OEM_MINUS ;

                default:
                    return null;
            }
        }
    }
}
