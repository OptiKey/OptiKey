using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    /// <summary>
    /// Voice command source is using Window's speech recognition engine to detect spocken command, 
    /// and consider them as triggers
    /// </summary>
    public class VoiceCommandSource : IVoiceCommandSource
    {
        private const string SemanticResultKey = "command";
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConfigurableCommandService commandService;
        private SpeechRecognitionEngine speechEngine;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        /// <summary>
        /// Builds the voice command source, delegating to ConfigurableCommandService the set of recognised commands
        /// </summary>
        /// <param name="configurableCommandService">Configurable command service instance</param>
        public VoiceCommandSource(IConfigurableCommandService configurableCommandService)
        {
            commandService = configurableCommandService;

            //Reload grammar when commands, voice command prefix, or language changes
            commandService.OnPropertyChanges(service => service.Commands).Subscribe(_ => ReloadGrammar());
            Settings.Default.OnPropertyChanges(settings => settings.VoiceCommandsPrefix).Subscribe(_ => ReloadGrammar());
            Settings.Default.OnPropertyChanges(settings => settings.ResourceLanguage).Subscribe(_ => ReloadGrammar());
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    sequence = Observable.Using(() =>
                        {
                            Log.Info("Initialising speech recognition engine.");
                            speechEngine = new SpeechRecognitionEngine();
                            ReloadGrammar();

                            //Starts recognition
                            speechEngine.SetInputToDefaultAudioDevice();
                            Log.Info("Starting speech recognition.");
                            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                            return speechEngine;
                        },
                        se => Observable.FromEventPattern<SpeechRecognizedEventArgs>(
                            h => se.SpeechRecognized += h,
                            h => se.SpeechRecognized -= h)
                            .Select(MatchRecognised)
                            .Where(_ => State == RunningStates.Running)
                            .Where(signal => 
                            {
                                //Filters empty signals, and in case of paused state, all commands except restart.
                                var keyValue = signal.PointAndKeyValue != null
                                    ? signal.PointAndKeyValue.Value.KeyValue
                                    : null;

                                if (keyValue == null) return false; //Key value is not useful

                                //Only publish the trigger signal if voice commands are currently enabled, or the signal is to (re-)start voice recognition
                                return Settings.Default.VoiceCommandsEnabled || keyValue.Value.FunctionKey == FunctionKeys.StartVoiceRecognition;
                            })
                        )
                        .Publish()
                        .RefCount();
                }

                return sequence;
            }
        }

        #endregion

        #region Internals

        /// <summary>
        /// Use ConfigurableCommandService instance to detect the command associated with the recognized input
        /// </summary>
        /// <param name="args">Recognition event</param>
        private TriggerSignal MatchRecognised(EventPattern<SpeechRecognizedEventArgs> args)
        {
            if (args != null && args.EventArgs != null && args.EventArgs.Result != null)
            {
                var semanticResult = args.EventArgs.Result.Semantics[SemanticResultKey];
                var key = (string)semanticResult.Value;

                Log.InfoFormat("Recognised speech pattern {0} for command {1}.", args.EventArgs.Result.Text, key);

                //Use the recognized pattern as notification, unless audio feedback is disabled
                var notification = Settings.Default.VoiceCommandsFeedback ? commandService.Commands[StringExtensions.Parse<FunctionKeys>(key)] : null;

                //Trigger the function key
                return new TriggerSignal(1, null, new PointAndKeyValue(new Point(0, 0), 
                    new KeyValue { FunctionKey = StringExtensions.Parse<FunctionKeys>(key) }
                ), notification);
            } 

            //Without recognized pattern, triggers an empty signal that will be discarded
            return new TriggerSignal();
        }

        /// <summary>
        /// Loads recognized grammar into the speech engine
        /// </summary>
        private void ReloadGrammar()
        {
            if (speechEngine == null)
            {
                Log.Warn("Reload grammar invoked, but speech engine hasn't been initialised.");
                return;
            }

            Log.Info("Reload speech recognition grammar.");
            speechEngine.UnloadAllGrammars();
            var commands = new Choices();
            foreach (var pair in commandService.Commands) 
            {
                commands.Add(new SemanticResultValue(pair.Value, pair.Key.ToString()));
                Log.Info(string.Format("Register pattern {0} for command {1}.", pair.Value, pair.Key.ToString()));
            }

            //Speech engine does not allow empty grammars so add a placeholder command if none exist
            if (commandService.Commands.Count == 0)
            {
                commands.Add("Supercalifragilisticexpialidocious");
            }

            //Use the predefined grammar prefix
            var grammarBuilder = new GrammarBuilder(Settings.Default.VoiceCommandsPrefix)
            {
                Culture = Settings.Default.ResourceLanguage.ToCultureInfo()
            };
            grammarBuilder.Append(new SemanticResultKey(SemanticResultKey, commands));
            speechEngine.LoadGrammar(new Grammar(grammarBuilder));
        }

        #endregion
    }
}
