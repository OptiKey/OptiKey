using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.ETTA.Properties;
using log4net;
using System.Speech.Synthesis;

namespace JuliusSweetland.ETTA.Services
{
    public class AudioService : IAudioService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Methods

        public void Speak(string textToSpeak, int? volume = null, int? rate = null, string voice = null)
        {
            try
            {
                Log.Debug(string.Format("Speaking '{0}' with volume '{1}', rate '{2}' and voice '{3}'", 
                    textToSpeak, volume, rate, voice));

                var speechSynthesiser = new SpeechSynthesizer
                {
                    Rate = rate ?? Settings.Default.SpeechRate,
                    Volume = volume ?? Settings.Default.SpeechVolume
                };

                var voiceToUse = voice ?? Settings.Default.SpeechVoice;

                if (!string.IsNullOrWhiteSpace(voiceToUse))
                {
                    try
                    {
                        speechSynthesiser.SelectVoice(voiceToUse);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(string.Format("There was a problem setting the voice to '{0}'{1}", voiceToUse, voice == null ? " (from settings)" : null), exception);

                        Settings.Default.SpeechVoice = null;

                        if (Error != null)
                        {
                            Error(this, new ApplicationException(string.Format("There was a problem setting the voice to '{0}'{1}. Resetting back to the default voice for the moment. Please correct the setting.", voiceToUse, voice == null ? " (from settings)" : null)));
                        }
                    }
                }

                speechSynthesiser.SpeakAsync(textToSpeak);
            }
            catch (Exception exception)
            {
                Log.Error("Speak threw an exception", exception);

                if (Error != null)
                {
                    Error(this, exception);
                }
            }
        }

        public List<string> GetAvailableVoices()
        {
            return new SpeechSynthesizer().GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => v.VoiceInfo.Name)
                .ToList();
        }

        public void PlaySound(string soundLocation)
        {
            try
            {
                var player = new System.Media.SoundPlayer(soundLocation);
                player.Play();
            }
            catch (Exception exception)
            {
                Log.Error("PlaySound threw an exception", exception);

                if (Error != null)
                {
                    Error(this, exception);
                }
            }
        }

        #endregion
    }
}
