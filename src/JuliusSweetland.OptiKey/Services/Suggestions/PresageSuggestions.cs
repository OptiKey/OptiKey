using JuliusSweetland.OptiKey.Models;
using log4net;
using System.Collections.Generic;
using System.Linq;
using presage;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    public class PresageSuggestions : IManagedSuggestions
    {
        private readonly Dictionary<string, HashSet<DictionaryEntry>> entries = new Dictionary<string, HashSet<DictionaryEntry>>();
        private readonly HashSet<string> wordsIndex = new HashSet<string>();

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Presage prsg;

        private string root = "";

        public PresageSuggestions()
        {
            prsg = new Presage(this.callback_get_past_stream, this.callback_get_future_stream);
        }

        /// <summary>
        /// Removes all possible suggestions from the auto complete provider.
        /// </summary>
        public void Clear()
        {
            Log.Debug("Clear called.");
        }

        public IEnumerable<string> GetSuggestions(string root, bool nextWord)
        {
            Log.DebugFormat("GetSuggestions called with root '{0}'", root);

            if (root == null)
                this.root = "";
            else
            {
                this.root = root;
                // force presage to suggest the next word by adding a space 
                if (nextWord && root.Length > 0 && char.IsLetterOrDigit(root.Last()))
                {
                    this.root = this.root + " ";
                }
            }

            if (prsg != null)
            {
                return prsg.predict();
            }

            return Enumerable.Empty<string>();
        }

        public Dictionary<string, HashSet<DictionaryEntry>> GetEntries()
        {
            return entries;
        }

        public HashSet<string> GetWordsHashes()
        {
            return wordsIndex;
        }

        public void AddEntry(string entry, DictionaryEntry dictionaryEntry, string normalizedHash = "")
        {
        }
        
        public void AddEntry(string entry, DictionaryEntry metaData)
        {
        }

        public void RemoveEntry(string entry)
        {
        }

        private string callback_get_past_stream()
        {
            int l = root.Length > 500 ? root.Length - 500 : 0;
            return root.Substring(l);
        }

        private string callback_get_future_stream()
        {
            return "";
        }
    }
}
