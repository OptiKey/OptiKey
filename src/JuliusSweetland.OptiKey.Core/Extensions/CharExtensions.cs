// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Globalization;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class CharExtensions
    {
        private static readonly Map<char, char> HangulIntialToFinalConsonents = new Map<char, char>();
        private static readonly Map<char, char> HiraganaUpperToLowerCase = new Map<char, char>();
        private static readonly Map<char, char> KatakanaUpperToLowerCase = new Map<char, char>();
        
        static CharExtensions()
        {
            //Korean mappings
            HangulIntialToFinalConsonents.Add('\u1100', '\u11A8'); //ㄱ
            HangulIntialToFinalConsonents.Add('\u1101', '\u11A9'); //ㄲ
            HangulIntialToFinalConsonents.Add('\u1102', '\u11AB'); //ㄴ
            HangulIntialToFinalConsonents.Add('\u1103', '\u11AE'); //ㄷ
            HangulIntialToFinalConsonents.Add('\u1105', '\u11AF'); //ㄹ
            HangulIntialToFinalConsonents.Add('\u1106', '\u11B7'); //ㅁ
            HangulIntialToFinalConsonents.Add('\u1107', '\u11B8'); //ㅂ
            HangulIntialToFinalConsonents.Add('\u1109', '\u11BA'); //ㅅ
            HangulIntialToFinalConsonents.Add('\u110A', '\u11BB'); //ㅆ
            HangulIntialToFinalConsonents.Add('\u110B', '\u11BC'); //ㅇ
            HangulIntialToFinalConsonents.Add('\u110C', '\u11BD'); //ㅈ
            HangulIntialToFinalConsonents.Add('\u110E', '\u11BE'); //ㅊ
            HangulIntialToFinalConsonents.Add('\u110F', '\u11BF'); //ㅋ
            HangulIntialToFinalConsonents.Add('\u1110', '\u11C0'); //ㅌ
            HangulIntialToFinalConsonents.Add('\u1111', '\u11C1'); //ㅍ
            HangulIntialToFinalConsonents.Add('\u1112', '\u11C2'); //ㅎ

            //Japanese Hiragan mappings
            HiraganaUpperToLowerCase.Add('あ', 'ぁ'); //あ
            HiraganaUpperToLowerCase.Add('い', 'ぃ'); //い
            HiraganaUpperToLowerCase.Add('う', 'ぅ'); //う
            HiraganaUpperToLowerCase.Add('え', 'ぇ'); //え
            HiraganaUpperToLowerCase.Add('お', 'ぉ'); //お
            HiraganaUpperToLowerCase.Add('つ', 'っ'); //つ
            HiraganaUpperToLowerCase.Add('や', 'ゃ'); //や
            HiraganaUpperToLowerCase.Add('ゆ', 'ゅ'); //ゆ
            HiraganaUpperToLowerCase.Add('よ', 'ょ'); //よ
            HiraganaUpperToLowerCase.Add('わ', 'ゎ'); //わ

            //Japanese Katakana mappings
            KatakanaUpperToLowerCase.Add('ア', 'ァ'); //ア
            KatakanaUpperToLowerCase.Add('イ', 'ィ'); //イ
            KatakanaUpperToLowerCase.Add('ウ', 'ゥ'); //ウ
            KatakanaUpperToLowerCase.Add('エ', 'ェ'); //エ
            KatakanaUpperToLowerCase.Add('オ', 'ォ'); //オ
            KatakanaUpperToLowerCase.Add('カ', 'ヵ'); //カ
            KatakanaUpperToLowerCase.Add('ク', 'ㇰ'); //ク
            KatakanaUpperToLowerCase.Add('ケ', 'ヶ'); //ケ
            KatakanaUpperToLowerCase.Add('シ', 'ㇱ'); //シ
            KatakanaUpperToLowerCase.Add('ス', 'ㇲ'); //ス
            KatakanaUpperToLowerCase.Add('ツ', 'ッ'); //ツ
            KatakanaUpperToLowerCase.Add('ト', 'ㇳ'); //ト
            KatakanaUpperToLowerCase.Add('ヌ', 'ㇴ'); //ヌ
            KatakanaUpperToLowerCase.Add('ハ', 'ㇵ'); //ハ
            KatakanaUpperToLowerCase.Add('ヒ', 'ㇶ'); //ヒ
            KatakanaUpperToLowerCase.Add('フ', 'ㇷ'); //フ
            KatakanaUpperToLowerCase.Add('ヘ', 'ㇸ'); //ヘ
            KatakanaUpperToLowerCase.Add('ホ', 'ㇹ'); //ホ
            KatakanaUpperToLowerCase.Add('ム', 'ㇺ'); //ム
            KatakanaUpperToLowerCase.Add('ヤ', 'ャ'); //ヤ
            KatakanaUpperToLowerCase.Add('ユ', 'ュ'); //ユ
            KatakanaUpperToLowerCase.Add('ヨ', 'ョ'); //ヨ
            KatakanaUpperToLowerCase.Add('ラ', 'ㇻ'); //ラ
            KatakanaUpperToLowerCase.Add('リ', 'ㇼ'); //リ
            KatakanaUpperToLowerCase.Add('ル', 'ㇽ'); //ル
            KatakanaUpperToLowerCase.Add('レ', 'ㇾ'); //レ
            KatakanaUpperToLowerCase.Add('ロ', 'ㇿ'); //ロ
            KatakanaUpperToLowerCase.Add('ワ', 'ヮ'); //ワ
        }
        
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

            if (char.IsLetterOrDigit(c) || char.IsSymbol(c) || char.IsPunctuation(c) 
                || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                return CharCategories.LetterOrDigitOrSymbolOrPunctuation;
            }

            return CharCategories.SomethingElse;
        }

        public static UnicodeCodePointRanges UnicodeCodePointRange(this char c)
        {
            var codePoint = (int)c;

            if (codePoint >= 0xAC00 && codePoint <= 0xD7A3)
            {
                return UnicodeCodePointRanges.HangulSyllable;
            }

            if (codePoint >= 0x1100 && codePoint <= 0x1112)
            {
                return UnicodeCodePointRanges.HangulInitialConsonant;
            }

            if (codePoint >= 0x1161 && codePoint <= 0x1175)
            {
                return UnicodeCodePointRanges.HangulVowel;
            }

            if (codePoint >= 0x11A8 && codePoint <= 0x11C2)
            {
                return UnicodeCodePointRanges.HangulFinalConsonant;
            }

            return UnicodeCodePointRanges.Other;
        }

        #region Hangul (Korean Character) Extension Methods

        public static bool CanBeInitialOrFinalHangulConsonant(this char c)
        {
            return HangulIntialToFinalConsonents.Forward.ContainsKey(c)
                   || HangulIntialToFinalConsonents.Reverse.ContainsKey(c);
        }

        public static char ConvertToInitialHangulConsonant(this char c)
        {
            if(c.UnicodeCodePointRange() == UnicodeCodePointRanges.HangulFinalConsonant
                && HangulIntialToFinalConsonents.Reverse.ContainsKey(c))
            {
                return HangulIntialToFinalConsonents.Reverse[c];
            }
            return c;
        }

        public static char ConvertToFinalHangulConsonant(this char c)
        {
            if (c.UnicodeCodePointRange() == UnicodeCodePointRanges.HangulInitialConsonant
                && HangulIntialToFinalConsonents.Forward.ContainsKey(c))
            {
                return HangulIntialToFinalConsonents.Forward[c];
            }
            return c;
        }

        #endregion

        public static char ToggleCase(this char c)
        {
            //Upper to lower conversions:

            if (HiraganaUpperToLowerCase.Forward.ContainsKey(c))
            {
                return HiraganaUpperToLowerCase.Forward[c];
            }

            if (KatakanaUpperToLowerCase.Forward.ContainsKey(c))
            {
                return KatakanaUpperToLowerCase.Forward[c];
            }

            if (char.IsUpper(c))
            {
                return char.ToLowerInvariant(c);
            }

            //Lower to upper conversions

            if (HiraganaUpperToLowerCase.Reverse.ContainsKey(c))
            {
                return HiraganaUpperToLowerCase.Reverse[c];
            }

            if (KatakanaUpperToLowerCase.Reverse.ContainsKey(c))
            {
                return KatakanaUpperToLowerCase.Reverse[c];
            }

            if (char.IsLower(c))
            {
                return char.ToUpperInvariant(c);
            }

            //Neither - just return the char
            return c;
        }

        public static string ToPrintableString(this char c)
        {
            var escapedLiteralString = c.ToString(CultureInfo.InvariantCulture)
                    .Replace("\0", @"\0")
                    .Replace("\a", @"\a")
                    .Replace("\b", @"\b")
                    .Replace("\t", @"\t")
                    .Replace("\f", @"\f")
                    .Replace("\n", @"\n")
                    .Replace("\r", @"\r");

            return string.Format(@"[Char:{0}|Unicode:U+{1:x4}]", escapedLiteralString, (int)c);
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
