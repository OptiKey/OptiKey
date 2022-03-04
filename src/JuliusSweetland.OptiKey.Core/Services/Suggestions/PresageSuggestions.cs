// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.Models;
using log4net;
using presage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
                    this.root = UTF8EncodingToDefault(root);

                    // force presage to suggest the next word by adding a space 
                    if (nextWord && root.Length > 0 && char.IsLetterOrDigit(root.Last()))
                    {
                        this.root = this.root + " ";
                    }
                }

                if (prsg != null)
                {
                    List<string> suggestions = new List<string>();
                    foreach (string suggestion in prsg.predict())
                    {
                        suggestions.Add(defaultEncodingToUTF8(suggestion));
                    }
                    return suggestions;
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

        /// <summary>
        /// Buffer to hold byte representation of string for encoding conversion
        /// </summary>
        private byte[] _byteBuffer = new byte[10240];

        /// <summary>
        /// Converts default encoding to UTF8
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string defaultEncodingToUTF8(string input)
        {
            int length = Encoding.Default.GetBytes(input, 0, input.Length, _byteBuffer, 0);

            return Encoding.UTF8.GetString(_byteBuffer, 0, length).Replace('’', '\'');
        }

        /// <summary>
        /// Converts UTF8 encoded string to the default encoding
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>converted string</returns>
        protected string UTF8EncodingToDefault(string input)
        {
            int length = Encoding.UTF8.GetBytes(input.Replace('\'', '’'), 0, input.Length, _byteBuffer, 0);

            return Encoding.Default.GetString(_byteBuffer, 0, length).Replace('\'', '`');
        }
    }
}
