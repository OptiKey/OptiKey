using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using log4net;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class StringExtensions
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string CreateDictionaryEntryHash(this string entry, bool log = true)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                //Trim white space
                string hash = entry.Trim();

                //Phrase/Sentence - extract first letter of each word, from which we will build the hash
                if (hash.Contains(" "))
                {
                    hash = new string(hash
                        .Split(' ')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.First())
                        .ToArray());
                }

                //Only letters are relevent
                hash = new string(hash.Where(Char.IsLetter).ToArray());

                //Hashes are stored without diacritics (accents etc)
                hash = hash.RemoveDiacritics();

                //Hashes are stored as uppercase
                hash = hash.ToUpper();

                //Suppress substrings of repeated characters (keep only the first character)
                var hashStringBuilder = new StringBuilder();
                foreach (var c in hash.ToCharArray())
                {
                    if (hashStringBuilder.Length == 0 || hashStringBuilder[hashStringBuilder.Length - 1] != c)
                    {
                        hashStringBuilder.Append(c);
                    }
                }

                hash = hashStringBuilder.Length > 0 
                    ? hashStringBuilder.ToString()
                    : null;

                if (!string.IsNullOrWhiteSpace(hash))
                {
                    if (log)
                    {
                        Log.DebugFormat("User entered entry of '{0}' hashed to '{1}'", entry, hash);
                    }

                    return hash;
                }
            }

            return null;
        }

        public static string CreateAutoCompleteDictionaryEntryHash(this string entry, bool log = true)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                //Trim white space
                string hash = entry.Trim();
                
                //Hashes are stored without diacritics (accents etc)
                hash = hash.RemoveDiacritics();

                //Hashes are stored as uppercase
                hash = hash.ToUpper();
                
                if (!string.IsNullOrWhiteSpace(hash))
                {
                    if (log)
                    {
                        Log.DebugFormat("Entry of '{0}' hashed to '{1}'", entry, hash);
                    }

                    return hash;
                }
            }

            return null;
        }

        public static List<string> ExtractWordsAndLines(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var words = new Regex(Settings.Default.WordRegex).Matches(text)
                                .Cast<Match>()
                                .Select(match => match.Value.CleanupPossibleDictionaryEntry())
                                .Where(sanitisedMatch => sanitisedMatch != null)
                                .ToList();

            var lines = text.Split('\n')
                            .Select(line => line.CleanupPossibleDictionaryEntry())
                            .Where(sanitisedMatch => sanitisedMatch != null)
                            .ToList();

            var extracts = words.Concat(lines).Distinct().ToList();

            return extracts.Any() ? extracts : null;
        }

        public static string CleanupPossibleDictionaryEntry(this string word)
        {
            if (!string.IsNullOrWhiteSpace(word)
                && word.ToCharArray().Any(Char.IsLetter))
            {
                word = word.Trim();

                while (word.Length > 1
                    && !Char.IsLetterOrDigit(word.Last()))
                {
                    word = word.Substring(0, word.Length - 1);
                }

                if (word.Length > 1)
                {
                    return word;
                }
            }

            return null;
        }

        public static int LongestCommonSubsequence(this string str1, string str2)
        {
            int maxSubSeq = 0;

            //Create 2D Array
            var arr = new int[str1.Length + 1, str2.Length + 1];

            //Initialize 2D array 
            //Note only zeroth column and zeroth row is assigned value 0
            //Can be skipped as well as by default they are Zero in C#
            for (int i = 0; i < str2.Length; i++)
            {
                arr[0, i] = 0;
            }
            for (int i = 0; i < str1.Length; i++)
            {
                arr[i, 0] = 0;
            }
            //Start Filling the table 
            //If Character match add 1 to diagonal element of 2D array and fill it
            //Else put the max of the elements on its top or on its left
            //Keep track of the size using a variable "maxSubSeq"
            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                    {
                        //Character match add 1 to diagonal element of 2D array
                        arr[i, j] = arr[i - 1, j - 1] + 1;
                        if (arr[i, j] > maxSubSeq)
                        {
                            maxSubSeq = arr[i, j];
                        }
                    }
                    else
                    {
                        //Else put the max of the elements on its top or  on its left
                        arr[i, j] = Math.Max(arr[i, j - 1], arr[i - 1, j]);
                    }
                }
            }
            return maxSubSeq;
        }

        /// <summary>
        /// Remove diacritics (accents etc) from source string and returns the base string
        /// Info on unicode representation of diacritics: http://www.unicode.org/reports/tr15/
        /// � symbols in your dictionary file? Resave it in UTF-8 encoding (I use Notepad)
        /// </summary>
        public static string RemoveDiacritics(this string src, bool compatibilityDecomposition = true)
        {
            return RemoveDiacritics(src, compatibilityDecomposition, c => c);
        }

        /// <summary>
        /// Remove diacritics (accents etc) from source string and returns the base string
        /// Info on unicode representation of diacritics: http://www.unicode.org/reports/tr15/
        /// � symbols in your dictionary file? Resave it in UTF-8 encoding (I use Notepad) 
        /// </summary>
        public static string RemoveDiacritics(this string src, bool compatibilityDecomposition, Func<char, char> customFolding)
        {
            var sb = new StringBuilder();
            foreach (char c in Normalise(src, compatibilityDecomposition, customFolding))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static IEnumerable<char> Normalise(this string src, bool compatibilityDecomposition, Func<char, char> customFolding)
        {
            foreach (char c in src.Normalize(compatibilityDecomposition ? NormalizationForm.FormKD : NormalizationForm.FormD))
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        //do nothing
                        break;
                    default:
                        yield return customFolding(c);
                        break;
                }
            }
        }

        /// <summary>
        /// Returns an ordered list of char/int tuples with the input characters in order and the count from each repeating group
        /// e.g. ăăăBBc would convert to {[A,ă,3],[B,B,2],[C,c,1]}.
        /// Character comparisons use the default equality logic, so they are case sensitive.
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<char, char, int>> ToCharListWithCounts(this IEnumerable<string> input)
        {
            var result = new List<Tuple<char, char, int>>();

            var cleansedChars = input.Select(s => s.RemoveDiacritics().ToUpper().First()).ToList();
            var uncleansedChars = input.Select(s => s.First()).ToList();
            
            for (int index = 0; index < cleansedChars.Count; index++)
            {
                var cleansedCharacter = cleansedChars[index];
                var uncleansedCharacter = uncleansedChars[index];

                var count = 1;
                index++;
                while (index < cleansedChars.Count
                    && cleansedChars[index] == cleansedCharacter)
                {
                    count++;
                    index++;
                }
                
                result.Add(new Tuple<char, char, int>(cleansedCharacter, uncleansedCharacter, count));
                index--;
            }

            return result;
        }

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return string.Concat(input.First().ToString(CultureInfo.InvariantCulture).ToUpper(), input.Substring(1));
        }

        public static bool NextCharacterWouldBeStartOfNewSentence(this string input)
        {
            return string.IsNullOrEmpty(input)
                   || new[] {". ", "! ", "? ", "\n"}.Any(input.EndsWith);
        }

        public static string ConvertEscapedCharsToLiterals(this string input)
        {
            if (input == null) return null;

            return input
                .Replace("\0", @"\0")
                .Replace("\a", @"\a")
                .Replace("\b", @"\b")
                .Replace("\t", @"\t")
                .Replace("\f", @"\f")
                .Replace("\n", @"\n")
                .Replace("\r", @"\r");
        }

        public static int CountBackToLastCharCategoryBoundary(this string input)
        {
            int charsToRemove = 0;

            if (string.IsNullOrEmpty(input)) return charsToRemove;

            //Special case - LetterOrDigitOrSymbolOrPunctuation followed by single space - remove the final space before we start
            if (input.Length >= 2
                && input[input.Length - 1].ToCharCategory() == CharCategories.Space
                && input[input.Length - 2].ToCharCategory() == CharCategories.LetterOrDigitOrSymbolOrPunctuation)
            {
                charsToRemove = 1;
            }

            var charCategoryToRemove = input[input.Length - charsToRemove - 1].ToCharCategory();

            while (input.Length > charsToRemove
                    && input[input.Length - charsToRemove - 1].ToCharCategory() == charCategoryToRemove)
            {
                charsToRemove++;
            }

            Log.DebugFormat(
                "CountBackToLastCharCategoryBoundary called with '{0}' - boundary calculated as {1} characters from end.", input, charsToRemove);

            return charsToRemove;
        }

        public static string InProgressWord(this string input, int cursorIndex)
        {
            if (input != null
                && cursorIndex <= input.Length)
            {
                var startIndex = input.LastIndexOfAny(new[] { ' ', '\n' });

                if (startIndex == -1) startIndex = 0; //No word boundary found - start at the beginning of the string
                else startIndex++;
                return input.Substring(startIndex, cursorIndex - startIndex);
            }

            return null;
        }
    }
}