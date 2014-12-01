using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

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

        //http://inputsimulator.codeplex.com/SourceControl/latest#WindowsInput/Native/VirtualKeyCode.cs
        //http://msdn.microsoft.com/en-gb/library/windows/desktop/dd375731(v=vs.85).aspx
        public static VirtualKeyCodeSet? ToVirtualKeyCodeSet(this char character)
        {
            switch (character)
            {
                case 'a':
                case 'A':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_A}};

                case 'b':
                case 'B':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_B}};

                case 'c':
                case 'C':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_C}};

                case 'd':
                case 'D':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_D}};

                case 'e':
                case 'E':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_E}};

                case 'f':
                case 'F':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_F}};

                case 'g':
                case 'G':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_G}};

                case 'h':
                case 'H':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_H}};

                case 'i':
                case 'I':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_I}};

                case 'j':
                case 'J':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_J}};

                case 'k':
                case 'K':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_K}};

                case 'l':
                case 'L':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_L}};

                case 'm':
                case 'M':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_M}};

                case 'n':
                case 'N':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_N}};

                case 'o':
                case 'O':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_O}};

                case 'p':
                case 'P':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_P}};

                case 'q':
                case 'Q':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_Q}};

                case 'r':
                case 'R':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_R}};

                case 's':
                case 'S':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_S}};

                case 't':
                case 'T':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_T}};

                case 'u':
                case 'U':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_U}};

                case 'v':
                case 'V':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_V}};

                case 'w':
                case 'W':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_W}};

                case 'x':
                case 'X':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_X}};

                case 'y':
                case 'Y':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_Y}};

                case 'z':
                case 'Z':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_Z}};

                case '1':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_1}};

                case '2':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_2}};

                case '3':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_3}};

                case '4':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_4}};

                case '5':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_5}};

                case '6':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_6}};

                case '7':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_7}};

                case '8':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_8}};

                case '9':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_9}};

                case '0':
                    return new VirtualKeyCodeSet {KeyCodes = new List<VirtualKeyCode> {VirtualKeyCode.VK_0}};

                case ' ':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.SPACE } };

                case '\t':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.TAB } };

                case '\n':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.RETURN } };

                case ',':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.OEM_COMMA } };

                case '.':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.OEM_PERIOD } };

                case '+':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.OEM_PLUS } };

                case '-':
                    return new VirtualKeyCodeSet { KeyCodes = new List<VirtualKeyCode> { VirtualKeyCode.OEM_MINUS } };

                default:
                    return null;
            }
        }
    }
}
