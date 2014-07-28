using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using log4net;

namespace JuliusSweetland.ETTA.Services
{
    public class DictionaryService : IDictionaryService
    {
        #region Constants

        private const string ApplicationDataSubPath = @"JuliusSweetland\ETTA\";
        private const string UserDictionaryFileSuffix = "_customised";
        private const string OriginalDictionariesSubPath = @"Dictionaries\";
        private const string DictionaryFileType = ".dic";

        #endregion

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, List<DictionaryEntryWithUsageCount>> dictionary;

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
            try
            {
                dictionary = new Dictionary<string, List<DictionaryEntryWithUsageCount>>();

                //Load the user dictionary
                var userDictionaryPath = GetUserDictionaryPath();

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
                Log.Error("Loading dictionary from file threw an exception", exception);

                if (Error != null)
                {
                    Error(this, exception);
                }
            }
        }

        private void LoadOriginalDictionaryFromFile(string filePath)
        {
            Log.Debug(string.Format("Loading original dictionary from file '{0}'", filePath));

            using (var reader = File.OpenText(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Entries must be londer than 1 character
                    if (!string.IsNullOrWhiteSpace(line) 
                        && line.Trim().Length > 1)
                    {
                        AddEntryToDictionary(line.Trim(), isNewEntry: false, usageCount: 0);
                    }
                }
            }
        }

        private static string GetUserDictionaryPath()
        {
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            Directory.CreateDirectory(applicationDataPath); //Does nothing if already exists
            return Path.Combine(applicationDataPath, string.Format("{0}{1}{2}", Settings.Default.Language, UserDictionaryFileSuffix, DictionaryFileType));
        }

        private void LoadUserDictionaryFromFile(string filePath)
        {
            Log.Debug(string.Format("Loading user dictionary from file '{0}'", filePath));

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
                            AddEntryToDictionary(entry, isNewEntry: false, usageCount: usageCount);
                        }
                    }
                }
            }
        }

        private void SaveUserDictionaryToFile()
        {
            try
            {
                var userDictionaryPath = GetUserDictionaryPath();

                Log.Debug(string.Format("Saving user dictionary to file '{0}'", userDictionaryPath));

                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(userDictionaryPath);

                    foreach (var entryWithUsageCount in dictionary.SelectMany(pair => pair.Value))
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
                Log.Error("Saving user dictionary to file threw an exception:", exception);

                if (Error != null)
                {
                    Error(this, exception);
                }
            }
        }

        #endregion

        #region Exists In Dictionary

        public bool ExistsInDictionary(string entryToFind)
        {
            if (dictionary != null
                && !string.IsNullOrWhiteSpace(entryToFind))
            {
                return dictionary
                    .SelectMany(pair => pair.Value) //Expand out all values in the dictionary and all values in the sorted lists
                    .Select(dictionaryEntryWithUsageCount => dictionaryEntryWithUsageCount.Entry)
                    .Any(dictionaryEntry => !string.IsNullOrWhiteSpace(dictionaryEntry) && dictionaryEntry.Trim().Equals(entryToFind.Trim()));
            }

            return false;
        }

        #endregion

        #region Add New/Existing Entry to Dictionary

        public void AddNewEntryToDictionary(string entry)
        {
            AddEntryToDictionary(entry, isNewEntry: true, usageCount: 1);
        }

        private void AddEntryToDictionary(string entry, bool isNewEntry, int usageCount = 0)
        {
            if (dictionary != null
                && entry != null)
            {
                if (isNewEntry && ExistsInDictionary(entry)) //Don't add new custom entries if they already exist in the dictionary
                {
                    return;
                }

                //Add to in-memory (hashed) dictionary (and then save to custom dictionary file if new entry entered by user)
                var hash = entry.CreateDictionaryEntryHash(userEntered: isNewEntry);

                if (!string.IsNullOrWhiteSpace(hash))
                {
                    var newEntryWithUsageCount = new DictionaryEntryWithUsageCount { UsageCount = usageCount, Entry = entry };

                    if (dictionary.ContainsKey(hash))
                    {
                        if (dictionary[hash].All(nwwuc => nwwuc.Entry != entry))
                        {
                            dictionary[hash].Add(newEntryWithUsageCount);
                        }
                    }
                    else
                    {
                        dictionary.Add(hash, new List<DictionaryEntryWithUsageCount> { newEntryWithUsageCount });
                    }

                    if (isNewEntry)
                    {
                        Log.Debug(string.Format("Adding new (user supplied) entry '{0}' to dictionary and saving to disk", entry));

                        SaveUserDictionaryToFile();
                    }
                }
            }
        }

        #endregion

        #region Remove Entry From Dictionary

        public void RemoveEntryFromDictionary(string entry)
        {
            if (dictionary != null
                && !string.IsNullOrWhiteSpace(entry)
                && ExistsInDictionary(entry))
            {
                var hash = entry.CreateDictionaryEntryHash(userEntered: false);

                if (!string.IsNullOrWhiteSpace(hash)
                    && dictionary.ContainsKey(hash))
                {
                    var entryWithUsageCount = dictionary[hash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                    if (entryWithUsageCount != null)
                    {
                        Log.Debug(string.Format("Removing entry '{0}' from dictionary", entry));

                        dictionary[hash].Remove(entryWithUsageCount);

                        if (!dictionary[hash].Any())
                        {
                            dictionary.Remove(hash);
                        }
                    }
                }
            }
        }

        public void RemoveEntryFromDictionary(DictionaryEntryWithUsageCount entryWithUsageCount)
        {
            if (dictionary != null
                && entryWithUsageCount != null
                && !string.IsNullOrWhiteSpace(entryWithUsageCount.Entry)
                && ExistsInDictionary(entryWithUsageCount.Entry))
            {
                var hash = entryWithUsageCount.Entry.CreateDictionaryEntryHash(userEntered: false);

                if (!string.IsNullOrWhiteSpace(hash)
                    && dictionary.ContainsKey(hash))
                {
                    Log.Debug(string.Format("Removing entry '{0}' from dictionary", entryWithUsageCount.Entry));

                    dictionary[hash].Remove(entryWithUsageCount);

                    if (!dictionary[hash].Any())
                    {
                        dictionary.Remove(hash);
                    }

                    SaveUserDictionaryToFile();
                }
            }
        }

        #endregion

        #region Get Hashes

        public IEnumerable<string> GetHashes()
        {
            if (dictionary != null)
            {
                var enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    yield return pair.Key;
                }
            }
        }

        #endregion

        #region Get All Entries With Usage Counts

        public IEnumerable<DictionaryEntryWithUsageCount> GetAllEntriesWithUsageCounts()
        {
            if (dictionary != null)
            {
                var enumerator = dictionary
                    .SelectMany(pair => pair.Value)
                    .OrderBy(entryWithUsageCount => entryWithUsageCount.Entry).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        #endregion

        #region Get Entries By Hash

        public IEnumerable<string> GetEntries(string hash)
        {
            if (dictionary != null
                && dictionary.ContainsKey(hash))
            {
                var enumerator = dictionary[hash]
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

        #region Increment/Decrement Entry Usage Count

        public void IncrementEntryUsageCount(string entry)
        {
            PerformIncrementOrDecrementOfEntryUsageCount(entry, isIncrement: true);
        }

        public void DecrementEntryUsageCount(string entry)
        {
            PerformIncrementOrDecrementOfEntryUsageCount(entry, isIncrement: false);
        }

        private void PerformIncrementOrDecrementOfEntryUsageCount(string entry, bool isIncrement)
        {
            if (!string.IsNullOrWhiteSpace(entry)
                && dictionary != null)
            {
                var hash = entry.CreateDictionaryEntryHash(userEntered: true);

                if (hash != null
                    && dictionary.ContainsKey(hash))
                {
                    var usageCountAndEntry = dictionary[hash].FirstOrDefault(entryWithUsageCount => entryWithUsageCount.Entry == entry);
                    if (usageCountAndEntry != null)
                    {
                        if (isIncrement)
                        {
                            usageCountAndEntry.UsageCount++;
                        }
                        else
                        {
                            if (usageCountAndEntry.UsageCount > 0)
                            {
                                usageCountAndEntry.UsageCount--;
                            }
                        }

                        SaveUserDictionaryToFile();
                    }
                }
            }
        }

        #endregion
    }
}
