using System;
using System.Collections.Generic;
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
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConfigurableCommandService commandService;
        private SpeechRecognitionEngine speechEngine;
        private IObservable<TriggerSignal> sequence;
        private bool running = false;

        #endregion

        #region Ctor

        /// <summary>
        /// Builds the voice command source, delegating to ConfigurableCommandService the set of recognized commands
        /// </summary>
        /// <param name="configurableCommandService">Configurable command service instance</param>
        public VoiceCommandSource(
            IConfigurableCommandService configurableCommandService
        )
        {
            commandService = configurableCommandService;
            //On command, locale or prefix change, reloads grammar
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
                            Log.Debug("Initialize speech recognition engine.");
                            speechEngine = new SpeechRecognitionEngine();
                            ReloadGrammar();

                            //Starts recognition
                            speechEngine.SetInputToDefaultAudioDevice();
                            Log.Debug("Start speech recognition.");
                            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                            return speechEngine;
                        },
                        speechEngine => Observable.FromEventPattern<SpeechRecognizedEventArgs>(
                            h => speechEngine.SpeechRecognized += h,
                            h => speechEngine.SpeechRecognized -= h)
                            .Select(MatchRecognized)
                            .Where(signal => 
                            {
                                //Filters empty signals, and in cas of paused state, all commands except restart.
                                var keyValue = signal.PointAndKeyValue.Value.KeyValue;
                                return keyValue.HasValue && keyValue.Value.FunctionKey == FunctionKeys.StartVoiceRecognition || Settings.Default.VoiceCommandsEnabled && State == RunningStates.Running;
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
        /// <param name="evt">Recognition event</param>
        private TriggerSignal MatchRecognized(EventPattern<SpeechRecognizedEventArgs> evt)
        {
            if (evt != null && evt.EventArgs != null && evt.EventArgs.Result != null)
            {
                var semanticResult = evt.EventArgs.Result.Semantics["command"];
                var key = (string)semanticResult.Value;
                Log.Debug(string.Format("Recognized speech pattern {0} for command {1}.", evt.EventArgs.Result.Text, key));

                //Use the recognized pattern has notification, unless audio feedback is disabled
                var notification = Settings.Default.VoiceCommandsFeedback ? commandService.Commands[StringExtensions.Parse<FunctionKeys>(key)] : null;

                //Trigger the function key
                return new TriggerSignal(1, null, new PointAndKeyValue(
                    new Point(Cursor.Position.X, Cursor.Position.Y), new KeyValue { FunctionKey = StringExtensions.Parse<FunctionKeys>(key) }
                ), notification);
            } 
            else
            {
                //Without recognized pattern, triggers an empty signal that will be discarded
                return new TriggerSignal();
            }
        }

        /// <summary>
        /// Loads recognized grammar into the speech engine
        /// </summary>
        private void ReloadGrammar()
        {
            if (speechEngine == null)
            {
                Log.Warn("Reload grammar invoked while speech engine hasn't been initialized.");
            } 
            else
            {
                Log.Info("Reload speech recognition grammar.");
                speechEngine.UnloadAllGrammars();
                var commands = new Choices();
                foreach (var pair in commandService.Commands) 
                {
                    commands.Add(new SemanticResultValue(pair.Value, pair.Key.ToString()));
                    Log.Debug(string.Format("Register pattern {0} for command {1}.", pair.Value, pair.Key.ToString()));
                }

                //Avoid empty commands (speech engine does not allow empty grammars)
                if (commandService.Commands.Count == 0)
                {
                    commands.Add("Supercalifragilisticexpialidocious");
                }

                //Use the predefined grammar prefix
                var grammarBuilder = new GrammarBuilder(Settings.Default.VoiceCommandsPrefix);
                grammarBuilder.Culture = Settings.Default.ResourceLanguage.ToCultureInfo();
                grammarBuilder.Append(new SemanticResultKey("command", commands));
                speechEngine.LoadGrammar(new Grammar(grammarBuilder));
            }
        }

        #endregion
    }
}
