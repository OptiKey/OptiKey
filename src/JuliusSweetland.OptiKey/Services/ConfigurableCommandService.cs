using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class ConfigurableCommandService : BindableBase, IConfigurableCommandService
    {

        #region Constants

        private const string ApplicationDataPath = @"JuliusSweetland\OptiKey\VoiceCommands\";
        private const string DefaultCommandPath = @"VoiceCommands\";
        private const string CommandFileType = ".csv";

        #endregion
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        private Dictionary<FunctionKeys, string> commands;

        #endregion
        #region Fields and Events

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
            //Read from somewhere, with default values
            AsyncLoadFromFile();
        }

        #endregion
        #region Load / Save Commands

        /// <summary>
        /// Avoid synchronous loading or user won't be notifiy of any loading problems
        /// </summary>
        private async void AsyncLoadFromFile()
        {
            await Task.Delay(100);
            Load(Settings.Default.ResourceLanguage);
        }

        /// <summary>
        /// Populate voice commands by reading user custom command file for specified language.
        /// If user file does not exists, read default commands for this language and save them as user commands
        /// Various errors (file not exists, misformated or unknown command) will be reported
        /// </summary>
        /// <param name="language">Language to loas commands for</param>
        public void Load(Languages language)
        {
            Log.InfoFormat("LoadFromFile called. Language setting is '{0}'.", language);

            try
            {
                //Load user's commands
                var filePath = GetCommandFilePath(language);
                if (File.Exists(filePath))
                {
                    ReadFromFile(filePath);
                }
                else
                {
                    //Copy default commands to create user's commands
                    var defaultPath = Path.GetFullPath(string.Format(@"{0}{1}{2}", DefaultCommandPath, language, CommandFileType));

                    if (File.Exists(defaultPath))
                    {
                        //Read default values and create user file
                        ReadFromFile(defaultPath);
                        Save(language);
                    }
                    else
                    {
                        throw new ApplicationException(string.Format(Resources.NO_VOICE_COMMAND_FILE_ERROR, defaultPath));
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(new ApplicationException(string.Format(Resources.INVALID_VOICE_COMMAND_FILE_ERROR, exception.Message, language.ToDescription())));
            }
        }
        
        /// <summary>
        /// Effectively read a headless (no header row) CSV file to extract commands.
        /// Function key is at first column, string pattern at second
        /// </summary>
        /// <param name="filePath">Full path of CSV file</param>
        /// <exception cref="OverflowException">Unexisting function key used</exception>
        private void ReadFromFile(string filePath)
        {
            Log.InfoFormat("Loading voice commands from file '{0}'", filePath);

            var readCommands = new Dictionary<FunctionKeys, string>();
            var reader = new StreamReader(File.OpenRead(filePath));
            while (!reader.EndOfStream)
            {
                var values = (from value in reader.ReadLine().Split(';') select value.Trim()).ToArray() ;
                readCommands.Add(StringExtensions.Parse<FunctionKeys>(values[0]), values[1]);
                Log.DebugFormat("read command from file {0} {1}", values[0], values[1]);
            }
            //Use property Commands instead of private attribute commands to trigger PropertyChanged notification
            Commands = readCommands;
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
            return Path.Combine(root, string.Format("{0}{1}", language, CommandFileType));
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

                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(filePath);

                    foreach (var command in commands)
                    {
                        writer.WriteLine("{0};{1}", command.Key.ToString(), command.Value);
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
