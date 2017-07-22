using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Media;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Un4seen.Bass;

namespace JuliusSweetland.OptiKey.Services
{
    public class AudioService : IAudioService
    {
        #region Constants

        private const string BassRegistrationEmail = "optikeyfeedback@gmail.com";
        private const string BassRegistrationKey = "2X24252025152222";

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly SpeechSynthesizer speechSynthesiser;

        private readonly object speakCompletedLock = new object();
        private EventHandler<SpeakCompletedEventArgs> speakCompleted;
        
        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public AudioService()
        {
            speechSynthesiser = new SpeechSynthesizer();
            BassNet.Registration(BassRegistrationEmail, BassRegistrationKey);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Application.Current.Exit += (sender, args) => Bass.BASS_Free();
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Start speaking the supplied text, or cancel the in-progress speech
        /// </summary>
        /// <returns>Indication of whether speech is now in progress</returns>
        public bool SpeakNewOrInterruptCurrentSpeech(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null)
        {
            Log.Info("SpeakNewOrInterruptCurrentSpeech called");
            if (string.IsNullOrEmpty(textToSpeak)) return false;

            try
            {
                lock (speakCompletedLock)
                {
                    if (speakCompleted == null)
                    {
                        Speak(textToSpeak, onComplete, volume, rate, voice);
                        return true;
                    }
                    CancelSpeech();
                }
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
            return false;
        }

        public List<string> GetAvailableVoices()
        {
            Log.Info("GetAvailableVoices called");
            var availableVoices = new SpeechSynthesizer().GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => v.VoiceInfo.Name)
                .ToList();

            Log.InfoFormat("GetAvailableVoices returing {0} voices", availableVoices.Count);

            return availableVoices;
        }

        public void PlaySound(string file, int volume)
        {
            Log.InfoFormat("Playing sound '{0}' at volume '{1}'", file, volume);
            if (string.IsNullOrEmpty(file)) return;

            try
            {
                // create a stream channel from a file 
                int stream = Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE);
                if (stream != 0)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, (volume/100f));
                    Bass.BASS_ChannelPlay(stream, false);
                }
                else
                {
                    throw new ApplicationException(string.Format(Resources.BASS_UNABLE_TO_CREATE_STREAM_FROM_FILE, file));
                }
            }
            catch (Exception exception)
            {
                //Don't publish error - the error handler tries to play a sound file which could loop us right back here
                Log.Error("Exception encountered within the AudioService", exception);
            }
        }

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion

        #region Private methods

        private void CancelSpeech()
        {
            Log.Info("Cancelling all speech");
            lock (speakCompletedLock)
            {
                if (speakCompleted != null)
                {
                    speechSynthesiser.SpeakCompleted -= speakCompleted;
                    speakCompleted = null;
                }
                speechSynthesiser.SpeakAsyncCancelAll();
            }
        }

        private void Speak(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null)
        {
            Log.InfoFormat("Speaking '{0}' with volume '{1}', rate '{2}' and voice '{3}'", textToSpeak, volume, rate, voice);
            if (string.IsNullOrEmpty(textToSpeak)) return;

            if (false)
            {
                speechSynthesiser.Rate = rate ?? Settings.Default.SpeechRate;
                speechSynthesiser.Volume = volume ?? Settings.Default.SpeechVolume;

                var voiceToUse = voice ?? Settings.Default.SpeechVoice;
                if (!string.IsNullOrWhiteSpace(voiceToUse))
                {
                    try
                    {
                        speechSynthesiser.SelectVoice(voiceToUse);
                    }
                    catch (Exception exception)
                    {
                        var customException = new ApplicationException(string.Format(Resources.UNABLE_TO_SET_VOICE_WARNING,
                            voiceToUse, voice == null ? Resources.VOICE_COMES_FROM_SETTINGS : null), exception);
                        PublishError(this, customException);
                    }
                }

                speakCompleted = (sender, args) =>
                {
                    lock (speakCompletedLock)
                    {
                        speechSynthesiser.SpeakCompleted -= speakCompleted;
                        speakCompleted = null;
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    }
                };
                speechSynthesiser.SpeakCompleted += speakCompleted;
                speechSynthesiser.SpeakAsync(textToSpeak);
            }
            else
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = "http://localhost:59125/process?"
                    + "INPUT_TYPE=TEXT&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&"
                    + "LOCALE=en_GB&"
                    + "VOICE=dfki-spike-hsmm&"
                    + "INPUT_TEXT="+ textToSpeak
                    + "&effect_Volume_selected=on&effect_Volume_parameters=amount:1.0;";

                player.Load();
                player.Play();
                /*
                speakCompleted = null;
                if (onComplete != null)
                {
                    onComplete();
                }*/
            }

        }

        #endregion
    }
}
