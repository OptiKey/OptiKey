using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services.Suggestions;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Windows;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class DictionaryService : IDictionaryService
    {
        #region Constants

        private const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Dictionaries\";
        private const string OriginalDictionariesSubPath = @"Dictionaries\";
        private const string DictionaryFileType = ".dic";

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SuggestionMethods suggestionMethod;

		private Dictionary<string, HashSet<DictionaryEntry>> entries;
		private IManagedSuggestions managedSuggestions;
		#endregion

        #region Events

		public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public DictionaryService(SuggestionMethods suggestionMethod)
        {
            this.suggestionMethod = suggestionMethod;

            MigrateLegacyDictionaries();
            LoadDictionary();

            //Subscribe to changes in the keyboard language to reload the dictionary
            Settings.Default.OnPropertyChanges(settings => settings.KeyboardAndDictionaryLanguage).Subscribe(_ => LoadDictionary());
        }

        #endregion

        #region Migrate Legacy User Dictionaries

        private static void MigrateLegacyDictionaries()
        {
            var oldNewDictionaryFileNames = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("AmericanEnglish.dic", "EnglishUS.dic"),
                new Tuple<string, string>("BritishEnglish.dic", "EnglishUK.dic"),
                new Tuple<string, string>("CanadianEnglish.dic", "EnglishCanada.dic"),
            };

            foreach (var oldNewFileName in oldNewDictionaryFileNames)
            {
                var oldDictionaryPath = GetUserDictionaryPath(oldNewFileName.Item1);
                var oldFileInfo = new FileInfo(oldDictionaryPath);
                var newDictionaryPath = GetUserDictionaryPath(oldNewFileName.Item2);
                var newFileInfo = new FileInfo(newDictionaryPath);
                if (oldFileInfo.Exists)
                {
                    Log.InfoFormat("Old user dictionary file found at '{0}'", oldDictionaryPath);

                    //Delete new user dictionary
                    if (newFileInfo.Exists)
                    {
                        Log.InfoFormat("New user dictionary file also found at '{0}' - deleting this file", newDictionaryPath);
                        newFileInfo.Delete();
                    }

                    //Rename old user dictionary
                    Log.InfoFormat("Renaming old user dictionary file '{0}' to '{1}'", oldDictionaryPath, newDictionaryPath);
                    oldFileInfo.MoveTo(newDictionaryPath);
                }
            }
        }

        #endregion

        #region Load / Save Dictionary

        public void LoadDictionary()
        {
            Log.InfoFormat("LoadDictionary called. Keyboard language setting is '{0}'.", Settings.Default.KeyboardAndDictionaryLanguage);

            try
            {
                managedSuggestions = CreateSuggestions();

				// Create reference to the actual storage of the dictionary entries.
				entries = managedSuggestions.GetEntries();

                //Load the user dictionary
                var userDictionaryPath = GetUserDictionaryPath(Settings.Default.KeyboardAndDictionaryLanguage);

                if (File.Exists(userDictionaryPath))
                {
                    LoadUserDictionaryFromFile(userDictionaryPath);
                }
                else
                {
                    //Load the original dictionary
                    var originalDictionaryPath = Path.GetFullPath(string.Format(@"{0}{1}{2}", OriginalDictionariesSubPath, Settings.Default.KeyboardAndDictionaryLanguage, DictionaryFileType));

                    if (File.Exists(originalDictionaryPath))
                    {
                        LoadOriginalDictionaryFromFile(originalDictionaryPath);
                        SaveUserDictionaryToFile(); //Create a user specific version of the dictionary
                    }
                    else
                    {
                        throw new ApplicationException(string.Format(Resources.DICTIONARY_FILE_NOT_FOUND_ERROR, originalDictionaryPath));
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        private void LoadOriginalDictionaryFromFile(string filePath)
        {
            Log.DebugFormat("Loading original dictionary from file '{0}'", filePath);

            using (var reader = File.OpenText(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Entries must be londer than 1 character
                    if (!string.IsNullOrWhiteSpace(line)
                        && line.Trim().Length > 1)
                    {
                        AddEntryToDictionary(line.Trim(), loadedFromDictionaryFile: true, usageCount: 0);
                    }
                }
            }
        }

        private static string GetUserDictionaryPath(Languages? language)
        {
            return GetUserDictionaryPath(string.Format("{0}{1}", language, DictionaryFileType));
        }

        private static string GetUserDictionaryPath(string fileName)
        {
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            Directory.CreateDirectory(applicationDataPath); //Does nothing if already exists
            return Path.Combine(applicationDataPath, fileName);
        }

        private void LoadUserDictionaryFromFile(string filePath)
        {
            Log.DebugFormat("Loading user dictionary from file '{0}'", filePath);

            using (var reader = File.OpenText(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var entryWithUsageCount = line.Trim().Split('|');
                        if (entryWithUsageCount.Length == 2)
                        {
                            var entry = entryWithUsageCount[0];
                            var usageCount = int.Parse(entryWithUsageCount[1]);
                            AddEntryToDictionary(entry, loadedFromDictionaryFile: true, usageCount: usageCount);
                        }
                    }
                }
            }
        }

        private void SaveUserDictionaryToFile()
        {
            try
            {
                var userDictionaryPath = GetUserDictionaryPath(Settings.Default.KeyboardAndDictionaryLanguage);

                Log.DebugFormat("Saving user dictionary to file '{0}'", userDictionaryPath);

                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(userDictionaryPath);

                    foreach (var entryWithUsageCount in entries.SelectMany(pair => pair.Value))
                    {
                        writer.WriteLine("{0}|{1}", entryWithUsageCount.Entry, entryWithUsageCount.UsageCount);
                    }
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        #endregion

        #region Exists In Dictionary

        public bool ExistsInDictionary(string entryToFind)
        {
            Log.DebugFormat("ExistsInDictionary called with '{0}'.", entryToFind);

            if (entries != null
                && !string.IsNullOrWhiteSpace(entryToFind))
            {
                var exists = entries
                    .SelectMany(pair => pair.Value) //Expand out all values in the dictionary and all values in the sorted lists
                    .Select(dictionaryEntryWithUsageCount => dictionaryEntryWithUsageCount.Entry)
                    .Any(dictionaryEntry => !string.IsNullOrWhiteSpace(dictionaryEntry) && dictionaryEntry.Trim().Equals(entryToFind.Trim()));

                Log.Debug(exists
                    ? string.Format("'{0}' exists in the dictionary", entryToFind)
                    : string.Format("'{0}' does not exist in the dictionary", entryToFind));

                return exists;
            }

            return false;
        }

        #endregion

        #region Add New/Existing Entry to Dictionary

        public void AddNewEntryToDictionary(string entry)
        {
            AddEntryToDictionary(entry, loadedFromDictionaryFile: false, usageCount: 1);
        }

        private void AddEntryToDictionary(string entry, bool loadedFromDictionaryFile, int usageCount = 0)
        {
            if (entries != null
                && !string.IsNullOrWhiteSpace(entry)
                && (loadedFromDictionaryFile || !ExistsInDictionary(entry)))
            {
                //Add to in-memory (hashed) dictionary (and then save to custom dictionary file if new entry entered by user)
                var hash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: !loadedFromDictionaryFile);
                if (!string.IsNullOrWhiteSpace(hash))
                {
                    var newEntryWithUsageCount = new DictionaryEntry(entry, usageCount);

					//Also add to entries for auto complete
					managedSuggestions.AddEntry(entry, newEntryWithUsageCount, hash);

                    if (!loadedFromDictionaryFile)
                    {
                        Log.DebugFormat("Adding new (not loaded from dictionary file) entry '{0}' to in-memory dictionary with hash '{1}'", entry, hash);
                        SaveUserDictionaryToFile();
                    }
                }
            }
        }

        #endregion

        #region Remove Entry From Dictionary

        public void RemoveEntryFromDictionary(string entry)
        {
            Log.DebugFormat("RemoveEntryFromDictionary called with entry '{0}'", entry);

            if (entries != null
                && !string.IsNullOrWhiteSpace(entry)
                && ExistsInDictionary(entry))
            {
                var hash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
                if (!string.IsNullOrWhiteSpace(hash)
                    && entries.ContainsKey(hash))
                {
                    var foundEntry = entries[hash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                    if (foundEntry != null)
                    {
                        Log.DebugFormat("Removing entry '{0}' from dictionary", entry);

						//Also remove from entries for auto complete
						managedSuggestions.RemoveEntry(entry);

                        SaveUserDictionaryToFile();
                    }
                }
            }
        }

        #endregion

        #region Get All Entries With Usage Counts

        public IEnumerable<DictionaryEntry> GetAllEntries()
        {
            Log.Debug("GetAllEntries called.");

            if (entries != null)
            {
                var enumerator = entries
                    .SelectMany(entry => entry.Value)
                    .OrderBy(entryWithUsageCount => entryWithUsageCount.Entry)
                    .GetEnumerator();

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        #endregion

        #region Get Suggestions

        public IEnumerable<string> GetSuggestions(string root, bool nextWord)
        {
            return managedSuggestions.GetSuggestions(root, nextWord);
        }

        #endregion

        #region Increment/Decrement Entry Usage Count

        public void IncrementEntryUsageCount(string entry)
        {
            IncrementOrDecrementOfEntryUsageCount(entry, isIncrement: true);
        }

        public void DecrementEntryUsageCount(string entry)
        {
            IncrementOrDecrementOfEntryUsageCount(entry, isIncrement: false);
        }

        private void IncrementOrDecrementOfEntryUsageCount(string text, bool isIncrement)
        {
            Log.DebugFormat("PerformIncrementOrDecrementOfEntryUsageCount called with entry '{0}' and isIncrement={1}", text, isIncrement);

            if (!string.IsNullOrWhiteSpace(text)
                && entries != null)
            {
                var hash = text.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);

                if (hash != null
                    && entries.ContainsKey(hash))
                {
                    bool saveDictionary = false;

                    var matches = new List<DictionaryEntry>();
                    var exactMatch = entries[hash].FirstOrDefault(de => de.Entry == text);
                    if (exactMatch != null)
                    {
                        matches.Add(exactMatch);
                    }
                    else if(!text.All(char.IsUpper))
                    {
                        //Text which is not all in caps could not have come from an all caps dictionary entry
                        var matchesWhichAreNotAllCaps = entries[hash].Where(dictionaryEntry =>
                            string.Equals(dictionaryEntry.Entry, text, StringComparison.InvariantCultureIgnoreCase)
                            && !dictionaryEntry.Entry.All(char.IsUpper));
                        matches.AddRange(matchesWhichAreNotAllCaps);
                    }
                    else
                    {
                        //The text does not have an exact match and is not all caps, so match ignoring case
                        var allInvariantMatches = entries[hash].Where(dictionaryEntry =>
                            string.Equals(dictionaryEntry.Entry, text, StringComparison.InvariantCultureIgnoreCase));
                        matches.AddRange(allInvariantMatches);
                    }

                    foreach (var match in matches)
                    {
                        if (isIncrement)
                        {
                            Log.DebugFormat("Incrementing the usage count of entry '{0}'.", match.Entry);
                            match.UsageCount++;
                        }
                        else
                        {
                            if (match.UsageCount > 0)
                            {
                                Log.DebugFormat("Decrementing the usage count of entry '{0}'.", match.Entry);
                                match.UsageCount--;
                            }
                            else
                            {
                                Log.Warn(string.Format("An attempt was made to decrement the usage count of entry '{0}', but the usage count was zero so no action was taken.", match.Entry));
                            }
                        }

                        saveDictionary = true;
                    }

                    if (saveDictionary)
                    {
                        SaveUserDictionaryToFile();
                    }
                }
            }
        }

        #endregion

        #region Map Capture To Entries

        public Tuple<List<Point>, FunctionKeys?, string, List<string>> MapCaptureToEntries(
            List<Timestamped<PointAndKeyValue>> timestampedPointAndKeyValues,
            int minCount, string reliableFirstLetter, string reliableLastLetter,
            ref CancellationTokenSource cancellationTokenSource, Action<Exception> exceptionHandler)
        {
            try
            {
                Log.DebugFormat("Mapping capture to dictionary entries with {0} timestamped points/key values",
                    timestampedPointAndKeyValues != null ? timestampedPointAndKeyValues.Count : 0);

                var points = timestampedPointAndKeyValues == null
                    ? new List<Point>()
                    : timestampedPointAndKeyValues.Select(tp => tp.Value.Point).ToList();

                var charsWithCount = timestampedPointAndKeyValues != null
                    ? timestampedPointAndKeyValues
                        .Where(tp => tp.Value.StringIsLetter)
                        .Select(tp => tp.Value.String)
                        .ToCharListWithCounts()
                    : new List<Tuple<char, char, int>>();

                //Create strings (Item1==cleansed/hashed, Item2==uncleansed) of reliable + characters with counts above the mean
                var reliableFirstCharCleansed = reliableFirstLetter != null
                    ? reliableFirstLetter.Normalise().First()
                    : (char?)null;
                var reliableFirstCharUncleansed = reliableFirstLetter != null
                    ? reliableFirstLetter.First()
                    : (char?)null;
                var reliableLastCharCleansed = reliableLastLetter != null
                    ? reliableLastLetter.Normalise().First()
                    : (char?)null;
                var reliableLastCharUncleansed = reliableLastLetter != null
                    ? reliableLastLetter.First()
                    : (char?)null;

                //Calculate threshold as mean of all letters without reliable first/last (as those selections can skew the average)
                var charsWithCountWithoutReliableFirstOrLast = charsWithCount.Where((cwc, index) =>
                {
                    if (index == 0 && reliableFirstCharCleansed != null && cwc.Item1 == reliableFirstCharCleansed.Value)
                        return false;

                    if (index == charsWithCount.Count - 1 && reliableLastCharCleansed != null && cwc.Item1 == reliableLastCharCleansed.Value)
                        return false;

                    return true;
                }).ToList();

                int thresholdCount = charsWithCountWithoutReliableFirstOrLast.Any()
                    ? Math.Max((int)Math.Floor(charsWithCountWithoutReliableFirstOrLast.Average(cwc => cwc.Item3)), minCount) //Coerce threshold up to minimum count from settings
                    : minCount;

                var filteredStrings = ToCleansedUncleansedStrings(charsWithCount, thresholdCount,
                    reliableFirstCharCleansed, reliableFirstCharUncleansed, reliableLastCharCleansed, reliableLastCharUncleansed);

                if (filteredStrings.Item1.Length == 0)
                {
                    Log.Info("Capture reduces to nothing useful.");
                    return new Tuple<List<Point>, FunctionKeys?, string, List<string>>(points, null, null, null);
                }

                if (filteredStrings.Item1.Length == 1)
                {
                    Log.Info("Capture reduces to a single letter.");
                    return new Tuple<List<Point>, FunctionKeys?, string, List<string>>(points, null, filteredStrings.Item2, null);
                }

                Log.DebugFormat("Attempting to match using filtered string: '{0}'", filteredStrings.Item1);

                cancellationTokenSource = new CancellationTokenSource();
                var matches = new List<string>();

                GetHashes()
                    .AsParallel()
                    .WithCancellation(cancellationTokenSource.Token)
                    .Where(hash => reliableFirstCharCleansed == null || hash.First() == reliableFirstCharCleansed.Value)
                    .Where(hash => reliableLastCharCleansed == null || hash.Last() == reliableLastCharCleansed.Value)
                    .Select(hash =>
                    {
                        var lcs = filteredStrings.Item1.LongestCommonSubsequence(hash);

                        return new
                        {
                            Hash = hash,
                            HashLastLetter = hash.Last(),
                            CaptureLastLetter = filteredStrings.Item1.Last(),
                            SimilarityToMeanFilteredString = ((double)lcs / (double)hash.Length) * (double)lcs
                        };
                    })
                    .OrderByDescending(x => x.SimilarityToMeanFilteredString)
                    .ThenByDescending(x => x.HashLastLetter == x.CaptureLastLetter) //Matching last letter - assume some reliability
                    .SelectMany(x => GetEntries(x.Hash))
                    .Take(Settings.Default.MaxDictionaryMatchesOrSuggestions)
                    .ToList()
                    .ForEach(matches.Add);

                matches.ForEach(match => Log.DebugFormat("Returning dictionary match: {0}", match));
                return new Tuple<List<Point>, FunctionKeys?, string, List<string>>(points, null, null, matches.Any() ? matches : null);
            }
            catch (OperationCanceledException)
            {
                Log.Error("Map capture to dictionary matches cancelled - returning nothing");
                return null;
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions != null
                    && ae.InnerExceptions.Any())
                {
                    var flattenedExceptions = ae.Flatten();
                    Log.Error("Aggregate exception encountered. Flattened exceptions:", flattenedExceptions);
                    exceptionHandler(flattenedExceptions);
                }
                return null;
            }
        }

        /// <summary>
        /// Construct a tuple of cleansed and original strings after applying a threshold to the count and ensuring the reliable first/last characters are present
        /// </summary>
        private Tuple<string, string> ToCleansedUncleansedStrings(IEnumerable<Tuple<char, char, int>> charsWithCount, int threshold,
            char? firstCharCleansed, char? firstCharUncleansed, char? lastCharCleansed, char? lastCharUncleansed)
        {
            var charsAboveThresold = charsWithCount.Where(cwc => (double) cwc.Item3 >= threshold)
                .Select(cwc => new Tuple<char, char>(cwc.Item1, cwc.Item2))
                .ToArray();

            var cleansedSb = new StringBuilder();
            var unCleansedSb = new StringBuilder();

            //Ensure first character is applied
            if (firstCharCleansed != null &&
                (!charsAboveThresold.Any() || charsAboveThresold.First().Item1 != firstCharCleansed.Value))
            {
                cleansedSb.Append(firstCharCleansed);
                unCleansedSb.Append(firstCharUncleansed);
            }

            cleansedSb.Append(new string(charsAboveThresold.Select(t => t.Item1).ToArray()));
            unCleansedSb.Append(new string(charsAboveThresold.Select(t => t.Item2).ToArray()));

            //Ensure last character is applied
            if (lastCharCleansed != null &&
                (!charsAboveThresold.Any() || charsAboveThresold.Last().Item1 != lastCharCleansed))
            {
                cleansedSb.Append(lastCharCleansed);
                unCleansedSb.Append(lastCharUncleansed);
            }

            return new Tuple<string, string>(cleansedSb.ToString(), unCleansedSb.ToString());
        }

        #endregion

        #region Private Methods

        #region Get Hashes

        private IEnumerable<string> GetHashes()
        {
            Log.Debug("GetHashes called.");

            if (entries != null)
            {
                var enumerator = entries.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    yield return pair.Key;
                }
            }
        }

        #endregion

        #region Get Entries

        private IEnumerable<string> GetEntries(string hash)
        {
            Log.DebugFormat("GetEntries called with hash '{0}'", hash);

            if (entries != null
                && entries.ContainsKey(hash))
            {
                var enumerator = entries[hash]
                    .OrderByDescending(entryWithUsageCount => entryWithUsageCount.UsageCount)
                    .Select(entryWithUsageCount => entryWithUsageCount.Entry)
                    .GetEnumerator();

                while (enumerator.MoveNext()
                    && !string.IsNullOrWhiteSpace(enumerator.Current))
                {
                    yield return enumerator.Current;
                }
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion

        #region Configure Auto Complete

        private IManagedSuggestions CreateSuggestions()
        {
            switch (suggestionMethod)
            {
                case SuggestionMethods.NGram:
                    return new NGramAutoComplete(
                        Settings.Default.NGramAutoCompleteGramCount,
                        Settings.Default.NGramAutoCompleteLeadingSpaceCount,
                        Settings.Default.NGramAutoCompleteTrailingSpaceCount);
                case SuggestionMethods.Presage:
                    return new PresageSuggestions();
                case SuggestionMethods.Basic:
                default:
                    return new BasicAutoComplete();
            }
        }

        #endregion

        #endregion
    }
}
