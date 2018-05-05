using System;
using JuliusSweetland.OptiKey.Models;
using log4net;
using presage;
using System.Collections.Generic;
using System.Linq;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
	public class PresageSuggestions : BasicAutoComplete
	{
		private readonly Dictionary<string, HashSet<DictionaryEntry>> entries = new Dictionary<string, HashSet<DictionaryEntry>>();
		private readonly HashSet<string> wordsIndex = new HashSet<string>();
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Presage prsg;
        private string root = "";

        public PresageSuggestions() : base()
        {
            prsg = new Presage(this.callback_get_past_stream, this.callback_get_future_stream);
        }

        public override IEnumerable<string> GetSuggestions(string root, bool nextWord)
        {
            Log.DebugFormat("GetSuggestions called with root '{0}'", root);

            try
            {
                if (root == null)
                {
                    this.root = "";
                }
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
            }
            catch (PresageException pe)
            {
                Log.Error("PresageException caught. Rethrowing. This is an attempt to see why the PresageException is not being caught. PresageException:", pe);
                throw;
            }
            
            return Enumerable.Empty<string>();
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
