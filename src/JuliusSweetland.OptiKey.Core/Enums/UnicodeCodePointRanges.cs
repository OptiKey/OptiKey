// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.Enums
{
    public enum UnicodeCodePointRanges
    {
        /// <summary>
        /// A range that OptiKey does not have specific logic for
        /// </summary>
        Other,

        /// <summary>
        /// A combined Hangul (Korean) syllable
        /// </summary>
        HangulSyllable,

        /// <summary>
        /// Initial (or leading) consonent (Ja-eum) jamo
        /// </summary>
        HangulInitialConsonant,

        /// <summary>
        /// Medial vowel (Mo-eum) jamo
        /// </summary>
        HangulVowel,

        /// <summary>
        /// Final (or trailing) consonent (Ja-eum) jamo
        /// </summary>
        HangulFinalConsonant
    }
}