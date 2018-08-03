using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services.Audio;
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
        private readonly SoundPlayerEx maryTtsPlayer;

        private readonly object speakCompletedLock = new object();
        private EventHandler<SpeakCompletedEventArgs> onSpeakCompleted;
        private EventHandler onMaryTtsSpeakCompleted;

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public AudioService()
        {
            speechSynthesiser = new SpeechSynthesizer();
            maryTtsPlayer = new SoundPlayerEx();
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
                    if (onSpeakCompleted == null && onMaryTtsSpeakCompleted == null)
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

        public List<string> GetAvailableMaryTTSVoices()
        {
            var availableVoices = new List<string>();

            if (Settings.Default.MaryTTSEnabled)
            {
                Log.Info("GetAvailableMaryTTSVoices called");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:59125/voices");

                //Set some reasonable limits on resources used by this request
                request.MaximumAutomaticRedirections = 4;
                request.MaximumResponseHeadersLength = 4;

                //Set credentials to use for this request.
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                //Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string responseText = readStream.ReadToEnd();
                availableVoices = responseText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                Log.InfoFormat("GetAvailableMaryTTSVoices returing {0} voices", availableVoices.Count);
            }

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
                if (onSpeakCompleted != null)
                {
                    speechSynthesiser.SpeakCompleted -= onSpeakCompleted;
                    onSpeakCompleted = null;
                }
                if (onMaryTtsSpeakCompleted != null)
                {
                    maryTtsPlayer.SoundFinished -= onMaryTtsSpeakCompleted;
                    onMaryTtsSpeakCompleted = null;
                }
                speechSynthesiser.SpeakAsyncCancelAll();
                maryTtsPlayer.Stop();
            }
        }

        private async void Speak(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null)
        {
            Log.InfoFormat("Speaking '{0}' with volume '{1}', rate '{2}' and voice '{3}'", textToSpeak, volume, rate, 
                !Settings.Default.MaryTTSEnabled ? voice : Settings.Default.MaryTTSVoice);

            if (string.IsNullOrEmpty(textToSpeak)) return;

            if (Settings.Default.SpeechDelay > 0)
            {
                await Task.Delay(Settings.Default.SpeechDelay);
            }

            if (!Settings.Default.MaryTTSEnabled)
            {
                //Default TTS
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

                onSpeakCompleted = (sender, args) =>
                {
                    lock (speakCompletedLock)
                    {
                        if (onSpeakCompleted != null)
                        {
                            speechSynthesiser.SpeakCompleted -= onSpeakCompleted;
                            onSpeakCompleted = null;
                        }
                        
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    }
                };
                speechSynthesiser.SpeakCompleted += onSpeakCompleted;
                speechSynthesiser.SpeakAsync(textToSpeak);
            }
            else
            {
                //MaryTTS
                float maryTTSRate = rate ?? Settings.Default.SpeechRate;
                float maryTTSVolume = volume ?? Settings.Default.SpeechVolume;

                maryTTSRate = (maryTTSRate + 10.0f) / 20.0f * 3.0f;
                maryTTSRate = maryTTSRate < 0.1f ? 0.1f
                    : maryTTSRate > 3.0f ? 3.0f : maryTTSRate;

                maryTTSVolume = maryTTSVolume / 100.0f * 1.0f;
                maryTTSVolume = maryTTSVolume < 0.0f ? 0.0f 
                    : maryTTSVolume > 1.0f ? 1.0f : maryTTSVolume;

                List<string> voiceParameters = Settings.Default.MaryTTSVoice.Split(' ').ToList();

                maryTtsPlayer.SoundLocation = "http://localhost:59125/process?"
                                              + "INPUT_TYPE=TEXT&AUDIO=WAVE_FILE&"
                                              + "LOCALE=" + voiceParameters.ElementAt(1) + "&"
                                              + "VOICE=" + voiceParameters.ElementAt(0) + "&"
                                              + "INPUT_TEXT=" + textToSpeak + "&"
                                              + "OUTPUT_TYPE=AUDIO&"
                                              + "effect_Rate_selected=on&effect_Rate_parameters=durScale:"
                                              + string.Format("{0:N1}", maryTTSRate) + ";&"
                                              + "effect_Volume_selected=on&effect_Volume_parameters=amount:"
                                              + string.Format("{0:N1}", maryTTSVolume) + ";";

                onMaryTtsSpeakCompleted = (sender, args) =>
                {
                    lock (speakCompletedLock)
                    {
                        maryTtsPlayer.SoundFinished -= onMaryTtsSpeakCompleted;
                        onMaryTtsSpeakCompleted = null;
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    }
                };

                maryTtsPlayer.SoundFinished += onMaryTtsSpeakCompleted;
#pragma warning disable 4014
                maryTtsPlayer.PlayAsync(ex => PublishError(this, ex)); //Awaitable, but we don't want to await it
#pragma warning restore 4014
            }
        }

        #endregion
    }
}
