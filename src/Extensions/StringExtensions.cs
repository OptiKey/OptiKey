using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using log4net;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class StringExtensions
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string CreateDictionaryEntryHash(this string entry, bool userEntered = true)
        {
            if (!string.IsNullOrWhiteSpace(entry))
            {
                //Trim white space
                string hash = entry.Trim();

                //Only letters are relevent
                hash = new string(hash.Where(Char.IsLetter).ToArray());

                //Phrase/Sentence - extract first letter of each word, from which we will build the hash
                if (hash.Contains(" "))
                {
                    hash = new string(hash
                        .Split(' ')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.First())
                        .ToArray());
                }

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
                    if (userEntered)
                    {
                        Log.Debug(string.Format("Entry '{0}' hashed to '{1}'", entry, hash));
                    }

                    return hash;
                }
            }

            return null;
        }

        public static List<string> MapToDictionaryMatches(this string capturedLetters,
            bool firstSequenceLetterIsReliable,
            bool lastSequenceLetterIsReliable,
            IDictionaryService dictionaryService,
            ref CancellationTokenSource cancellationTokenSource,
            Action<Exception> exceptionHandler)
        {
            var matches = new List<string>();

            try
            {
                if (capturedLetters != null && capturedLetters.Any())
                {
                    Log.Debug(string.Format("Searching for dictionary matches for captured letters: {0}", capturedLetters));

                    cancellationTokenSource = new CancellationTokenSource();

                    dictionaryService.GetHashes()
                        .AsParallel()
                        .WithCancellation(cancellationTokenSource.Token)
                        .Where(s => !firstSequenceLetterIsReliable || s.First() == capturedLetters.First())
                        .Where(s => !lastSequenceLetterIsReliable || s.Last() == capturedLetters.Last())
                        .Select(hash =>
                        {
                            var lcs = capturedLetters.LongestCommonSubsequence(hash);
                            return new
                            {
                                Hash = hash,
                                Similarity = ((double)lcs / (double)hash.Length) * lcs,
                                Lcs = lcs
                            };
                        })
                        .OrderByDescending(x => x.Similarity)
                        //.ThenBy(x => x.Hash.Length) //Shorter dictionary words are preferred (larger ratio of dictionary word which matches is better)
                        .ThenByDescending(x => x.Hash.Last() == capturedLetters.Last()) //Matching last letter
                        .SelectMany(x => dictionaryService.GetEntries(x.Hash))
                        .Take(Settings.Default.MultiKeySelectionMaxDictionaryMatches)
                        .ToList()
                        .ForEach(matches.Add);
                }

                if (matches.Any())
                {
                    matches.ForEach(match => Log.Debug(string.Format("Returning dictionary match: {0}", match)));
                    return matches;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Error("Map captured letters to dictionary matches cancelled - returning nothing");
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions != null)
                {
                    var flattenedExceptions = ae.Flatten();

                    Log.Error("Aggregate exception encountered. Flattened exception follows:", flattenedExceptions);

                    exceptionHandler(flattenedExceptions);
                }
            }

            return null;
        }

        public static int LongestCommonSubsequence(this string str1, string str2)
        {
            int maxSubSeq = 0;

            // Create 2-d Array
            var arr = new int[str1.Length + 1, str2.Length + 1];

            // Initialize 2 - d array 
            // Note only zeroth column and zeroth row is assigned value 0
            // Can be skipped as well as by default they are Zero in C#
            for (int i = 0; i < str2.Length; i++)
            {
                arr[0, i] = 0;
            }
            for (int i = 0; i < str1.Length; i++)
            {
                arr[i, 0] = 0;
            }
            // Start Filling the table 
            // if Character match add 1 to diagonal element of 2 - d array and fill it
            // Else put the max of the elements on its top or  on its left ..
            // Keep track of the size using a variable "maxSubSeq"
            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                    {
                        //Character match add 1 to diagonal element of 2 - d array
                        arr[i, j] = arr[i - 1, j - 1] + 1;
                        if (arr[i, j] > maxSubSeq)
                        {
                            maxSubSeq = arr[i, j];
                        }
                    }
                    else
                    {
                        // Else put the max of the elements on its top or  on its left ..
                        arr[i, j] = Math.Max(arr[i, j - 1], arr[i - 1, j]);
                    }
                }
            }
            return maxSubSeq;
        }

        /// <summary>
        /// Remove diacritics (accents etc) from source string and returns the base string
        /// Info on unicode representation of diacritics: http://www.unicode.org/reports/tr15/
        /// � symbols in your dictionary file? Resave it in UNICODE encoding 
        /// </summary>
        public static string RemoveDiacritics(this string src, bool compatibilityDecomposition = true)
        {
            return RemoveDiacritics(src, compatibilityDecomposition, c => c);
        }

        public static string RemoveDiacritics(this string src, bool compatibilityDecomposition, Func<char, char> customFolding)
        {
            var sb = new StringBuilder();
            foreach (char c in RemoveDiacriticsEnum(src, compatibilityDecomposition, customFolding))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static IEnumerable<char> RemoveDiacriticsEnum(this string src, bool compatibilityDecomposition, Func<char, char> customFolding)
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

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return string.Concat(input.First().ToString().ToUpper(), input.Substring(1));
        }

        public static bool NextCharacterWouldBeStartOfNewSentence(this string input)
        {
            return string.IsNullOrEmpty(input)
                   || new[] {". ", "! ", "? ", "\n"}.Any(input.EndsWith);
        }

        public static string ConvertEscapedCharsToLiterals(this string input)
        {
            return input
                .Replace("\0", @"\0")
                .Replace("\a", @"\a")
                .Replace("\b", @"\b")
                .Replace("\t", @"\t")
                .Replace("\f", @"\f")
                .Replace("\n", @"\n")
                .Replace("\r", @"\r");
        }
    }
}