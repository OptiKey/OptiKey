using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
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

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private Dictionary<string, List<DictionaryEntry>> entries;
        private Dictionary<string, List<DictionaryEntry>> entriesForAutoComplete;

        #endregion

        #region Events

        public event EventHandler<Exception> Error;
        
        #endregion

        #region Ctor

        public DictionaryService()
        {
            LoadDictionary();
        }

        #endregion

        #region Load / Save Dictionary

        public void LoadDictionary()
        {
            Log.DebugFormat("LoadDictionary called. Language setting is '{0}'.", Settings.Default.Language);

            try
            {
                entries = new Dictionary<string, List<DictionaryEntry>>();
                entriesForAutoComplete = new Dictionary<string, List<DictionaryEntry>>();

                //Load the user dictionary
                var userDictionaryPath = GetUserDictionaryPath(Settings.Default.Language);

                if (File.Exists(userDictionaryPath))
                {
                    LoadUserDictionaryFromFile(userDictionaryPath);
                }
                else
                {
                    //Load the original dictionary
                    var originalDictionaryPath = Path.GetFullPath(string.Format(@"{0}{1}{2}", OriginalDictionariesSubPath, Settings.Default.Language, DictionaryFileType));

                    if (File.Exists(originalDictionaryPath))
                    {
                        LoadOriginalDictionaryFromFile(originalDictionaryPath);
                        SaveUserDictionaryToFile(); //Create a user specific version of the dictionary
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("No user dictionary exists and original dictionary could not be found at path: '{0}'", originalDictionaryPath));
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
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            Directory.CreateDirectory(applicationDataPath); //Does nothing if already exists
            return Path.Combine(applicationDataPath, string.Format("{0}{1}", language, DictionaryFileType));
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
                var userDictionaryPath = GetUserDictionaryPath(Settings.Default.Language);

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
                var hash = entry.CreateDictionaryEntryHash(log: !loadedFromDictionaryFile);
                if (!string.IsNullOrWhiteSpace(hash))
                {
                    var newEntryWithUsageCount = new DictionaryEntry { UsageCount = usageCount, Entry = entry };

                    if (entries.ContainsKey(hash))
                    {
                        if (entries[hash].All(nwwuc => nwwuc.Entry != entry))
                        {
                            entries[hash].Add(newEntryWithUsageCount);
                        }
                    }
                    else
                    {
                        entries.Add(hash, new List<DictionaryEntry> { newEntryWithUsageCount });
                    }

                    //Also add to entries for auto complete
                    var autoCompleteHash = entry.CreateAutoCompleteDictionaryEntryHash(log: false);
                    if (!string.IsNullOrWhiteSpace(autoCompleteHash))
                    {
                        if (entriesForAutoComplete.ContainsKey(autoCompleteHash))
                        {
                            if (entriesForAutoComplete[autoCompleteHash].All(nwwuc => nwwuc.Entry != entry))
                            {
                                entriesForAutoComplete[autoCompleteHash].Add(newEntryWithUsageCount);
                            }
                        }
                        else
                        {
                            entriesForAutoComplete.Add(autoCompleteHash, new List<DictionaryEntry> { newEntryWithUsageCount });
                        }
                    }
                    
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
                var hash = entry.CreateDictionaryEntryHash(log: false);
                if (!string.IsNullOrWhiteSpace(hash)
                    && entries.ContainsKey(hash))
                {
                    var foundEntry = entries[hash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                    if (foundEntry != null)
                    {
                        Log.DebugFormat("Removing entry '{0}' from dictionary", entry);

                        entries[hash].Remove(foundEntry);

                        if (!entries[hash].Any())
                        {
                            entries.Remove(hash);
                        }

                        //Also remove from entries for auto complete
                        var autoCompleteHash = entry.CreateAutoCompleteDictionaryEntryHash(log: false);
                        if (!string.IsNullOrWhiteSpace(autoCompleteHash)
                            && entriesForAutoComplete.ContainsKey(autoCompleteHash))
                        {
                            var foundEntryForAutoComplete = entriesForAutoComplete[autoCompleteHash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                            if (foundEntryForAutoComplete != null)
                            {
                                entriesForAutoComplete[autoCompleteHash].Remove(foundEntryForAutoComplete);

                                if (!entriesForAutoComplete[autoCompleteHash].Any())
                                {
                                    entriesForAutoComplete.Remove(autoCompleteHash);
                                }
                            }
                        }

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

        #region Get Auto Complete Suggestions

        public IEnumerable<DictionaryEntry> GetAutoCompleteSuggestions(string root)
        {
            Log.DebugFormat("GetAutoCompleteSuggestions called with root '{0}'", root);

            if (entriesForAutoComplete != null)
            {
                var simplifiedRoot = root.CreateAutoCompleteDictionaryEntryHash();

                if (!string.IsNullOrWhiteSpace(simplifiedRoot))
                {
                    var enumerator = entriesForAutoComplete
                        .Where(kvp => kvp.Key.StartsWith(simplifiedRoot) && kvp.Key.Length > simplifiedRoot.Length)
                        .SelectMany(kvp => kvp.Value)
                        .OrderByDescending(de => de.UsageCount)
                        .ThenBy(de => de.Entry.Length)
                        .GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }

                yield break; //Not strictly necessary
            }
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
                var hash = text.CreateDictionaryEntryHash(log: false);

                if (hash != null
                    && entries.ContainsKey(hash))
                {
                    bool saveDictionary = false;

                    foreach (var match in entries[hash].Where(dictionaryEntry =>
                                string.Equals(dictionaryEntry.Entry, text, StringComparison.InvariantCultureIgnoreCase)))
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
                        .Select(tp => tp.Value.String.RemoveDiacritics().ToUpper().First())
                        .ToListWithCounts()
                    : new List<Tuple<char, int>>();
                
                char? reliableFirstChar = null;
                if (reliableFirstLetter != null)
                {
                    //Clean 1st letter - no accents and ensure uppercase
                    reliableFirstChar = reliableFirstLetter.RemoveDiacritics().ToUpper().First();
                }

                char? reliableLastChar = null;
                if (reliableLastLetter != null)
                {
                    //Clean last letter - no accents and ensure uppercase
                    reliableLastChar = reliableLastLetter.RemoveDiacritics().ToUpper().First();
                }

                //Create string of reliable first/last and chars above the min count threshold
                var thresholdFilteredString = ConvertToString(charsWithCount, minCount, reliableFirstChar, reliableLastChar);

                if (thresholdFilteredString.Length == 0)
                {
                    Log.Debug("Capture reduces to nothing useful.");
                    return new Tuple<List<Point>, FunctionKeys?, string, List<string>>(points, null, null, null);
                }

                if (thresholdFilteredString.Length == 1)
                {
                    Log.Debug("Capture reduces to a single letter.");
                    return new Tuple<List<Point>, FunctionKeys?, string, List<string>>(points, null, thresholdFilteredString, null);
                }

                //Create string of reliable first/last and chars above the mean count
                int meanCount = charsWithCount.Any() ? (int)Math.Floor(charsWithCount.Average(cwc => cwc.Item2)) : 0;
                var meanFilteredString = ConvertToString(charsWithCount, meanCount, reliableFirstChar, reliableLastChar);
                if (meanFilteredString.Length == 0 || meanFilteredString == thresholdFilteredString)
                {
                    meanFilteredString = null;
                }

                Log.DebugFormat("Attempting to match using meanFilteredString: '{0}' and thresholdFilteredString: '{1}'", meanFilteredString, thresholdFilteredString);

                cancellationTokenSource = new CancellationTokenSource();
                var matches = new List<string>();

                GetHashes()
                    .AsParallel()
                    .WithCancellation(cancellationTokenSource.Token)
                    .Where(hash => reliableFirstChar == null || hash.First() == reliableFirstChar.Value)
                    .Where(hash => reliableLastChar == null || hash.Last() == reliableLastChar.Value)
                    .Select(hash =>
                    {
                        var lcsWithThresholdFilteredString = thresholdFilteredString.LongestCommonSubsequence(hash);
                        var lcsWithThresholdAndMeanFilteredString = meanFilteredString == null 
                            ? 0
                            : meanFilteredString.LongestCommonSubsequence(hash);
                        
                        return new
                        {
                            Hash = hash,
                            HashLastLetter = hash.Last(),
                            CaptureLastLetter = reliableLastChar != null ? reliableLastChar.Value : thresholdFilteredString.Last(),
                            SimilarityToThresholdFiltered = ((double)lcsWithThresholdFilteredString / (double)hash.Length) * (double)lcsWithThresholdFilteredString,
                            SimilarityToThresholdAndMeanFilteredString = (double)lcsWithThresholdAndMeanFilteredString / (double)hash.Length
                        };
                    })
                    .OrderByDescending(x => x.SimilarityToThresholdAndMeanFilteredString)
                    .ThenByDescending(x => x.SimilarityToThresholdFiltered)
                    .ThenByDescending(x => x.HashLastLetter == x.CaptureLastLetter) //Matching last letter
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
            }

            return null;
        }

        private string ConvertToString(IEnumerable<Tuple<char, int>> charsWithCount, int threshold, char? firstChar, char? lastChar)
        {
            var sb = new StringBuilder();
            var charsAboveThresold = new string(charsWithCount.Where(cwc => (double) cwc.Item2 >= threshold).Select(cwc => cwc.Item1).ToArray());
            
            //Ensure first character is the 'firstChar' if supplied
            if (firstChar != null
                && (charsAboveThresold.Length == 0 || charsAboveThresold.First() != firstChar.Value))
            {
                sb.Append(firstChar.Value);
            }
            
            sb.Append(charsAboveThresold);

            //Ensure last character is the 'lastChar' if supplied
            if (lastChar != null
                && (charsAboveThresold.Length == 0 || charsAboveThresold.Last() != lastChar.Value))
            {
                sb.Append(lastChar.Value);
            }

            return sb.ToString();
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

        #endregion
    }
}
