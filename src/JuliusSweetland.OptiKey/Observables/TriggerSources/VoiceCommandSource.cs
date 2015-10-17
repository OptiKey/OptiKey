using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
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

        private SpeechRecognitionEngine speechEngine;
        private IObservable<TriggerSignal> sequence;

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

                            // starts recognition
                            speechEngine.SetInputToDefaultAudioDevice();
                            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                            return speechEngine;
                        },
                        speechEngine => Observable.FromEventPattern<SpeechRecognizedEventArgs>(
                            h => speechEngine.SpeechRecognized += h,
                            h => speechEngine.SpeechRecognized -= h)
                            .Repeat()
                            .Where(_ => State == RunningStates.Running)
                            .Select(evt =>
                            {
                                Log.Debug(string.Format("Recognized speech pattern {0}.", evt.EventArgs.Result.Text));
                                return new TriggerSignal(1, null, new PointAndKeyValue(new Point(Cursor.Position.X, Cursor.Position.Y), KeyValues.MouseMoveAndLeftClickKey));
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
                var commands = new Choices();
                commands.Add("click");
                var grammarBuilder = new GrammarBuilder();
                grammarBuilder.Append(new SemanticResultKey("command", commands));
                speechEngine.LoadGrammar(new Grammar(grammarBuilder));

            }
        }

        #endregion
    }
}
