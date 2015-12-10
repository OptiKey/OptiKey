using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class ConfigurableCommandService : BindableBase, IConfigurableCommandService
    {
        #region Constants

        internal const string ApplicationDataPath = @"JuliusSweetland\OptiKey\Commands\";
        internal const string CommandFileBase = "Voice";
        internal const string CommandFileType = ".csv";
        internal const string DefaultPath = @"JuliusSweetland.OptiKey.Properties.";

        #endregion

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        private Dictionary<FunctionKeys, string> commands;

        #endregion

        #region Properties

        public Dictionary<FunctionKeys, string> Commands
        {
            get { return commands; }
            set { SetProperty(ref commands, value); }
        }
        public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public ConfigurableCommandService()
        {
            commands = new Dictionary<FunctionKeys, string>();
        }

        #endregion

        #region Load / Save Commands

        /// <summary>
        /// Populate voice commands by reading user custom command file for specified language.
        /// If user file does not exists, read default commands for this language and save them as user commands.
        /// Various errors (file not exists, misformated or unknown command) will be reported.
        /// </summary>
        /// <param name="language">Language to loas commands for</param>
        public void Load(Languages language)
        {
            Log.InfoFormat("Load called. Language setting is '{0}'.", language);

            var readCommands = new Dictionary<FunctionKeys, string>();
            try
            {     
                //Load user's commands
                var filePath = GetCommandFilePath(language);
                var customExists = File.Exists(filePath);
                if (customExists)
                {
                    using (var reader = new StreamReader(File.OpenRead(filePath)))
                    {
                        while (!reader.EndOfStream)
                        {
                            var values = (from value in reader.ReadLine().Split(';') select value.Trim()).ToArray();
                            readCommands.Add(StringExtensions.Parse<FunctionKeys>(values[0]), values[1]);
                            Log.DebugFormat("read command from file {0} {1}", values[0], values[1]);
                        }
                    }
                }
                //Read default commands stored within assembly
                var resourceManager = new ResourceManager(string.Format("{0}{1}", DefaultPath, CommandFileBase), this.GetType().Assembly);
                var defaults = new Dictionary<FunctionKeys, string>();
                foreach(System.Collections.DictionaryEntry entry in resourceManager.GetResourceSet(language.ToCultureInfo(), true, true))
                {
                    defaults.Add(StringExtensions.Parse<FunctionKeys>((string)entry.Key), (string)entry.Value);
                }

                //Use property Commands instead of private attribute commands to trigger PropertyChanged notification
                //Merge all to ensure that added commands are taken into account
                var previousCount = readCommands.Count;
                Commands = readCommands.MergeLeft(defaults);
                if (Commands.Count > previousCount)
                {
                    //Update user file if needed
                    Save(language);
                }
            }
            catch (Exception exception)
            {
                PublishError(new ApplicationException(string.Format(Resources.INVALID_VOICE_COMMAND_FILE_ERROR, exception.Message, language.ToDescription())));
            }      
        }

        /// <summary>
        /// Returns full path of the CSV file containing language-specific commands
        /// Will creates the path if necessary.
        /// File is named after language name, with a csv extension
        /// </summary>
        /// <param name="language">Language for which commands are loaded</param>
        /// <returns>Full path of the read file</returns>
        private static string GetCommandFilePath(Languages language)
        {
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataPath);
            //Creates folder it it does not exists yet
            Directory.CreateDirectory(root);
            return Path.Combine(root, string.Format("{0}.{1}{2}", CommandFileBase, language.ToCultureInfo(), CommandFileType));
        }

        /// <summary>
        /// Rewrite the user custom file (current language dependent) with current commands
        /// </summary>
        /// <param name="language">Language for which commands are saved</param>
        public void Save(Languages language)
        {
            try
            {
                var filePath = GetCommandFilePath(language);
                Log.DebugFormat("Saving user dictionary to file '{0}'", filePath);

                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var command in commands)
                    {
                        writer.WriteLine("{0};{1}", command.Key.ToString(), command.Value);
                        Log.DebugFormat("read command from file {0} {1}", command.Key, command.Value);
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(exception);
            }
        }

        #endregion

        #region Publish Error

        /// <summary>
        /// Reporte error, by logging it and by publishing it on the event stream
        /// </summary>
        /// <param name="ex">Reported error</param>
        private void PublishError(Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(this, ex);
            }
        }

        #endregion
    }
}
