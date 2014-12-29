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
            if (string.IsNullOrEmpty(textToSpeak)) return;

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
                        var customException = new ApplicationException(
                            string.Format("There was a problem setting the voice to '{0}'{1}", 
                            voiceToUse, voice == null ? " (from settings)" : null), exception);

                        PublishError(this, customException);
                    }
                }

                speechSynthesiser.SpeakAsync(textToSpeak);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public List<string> GetAvailableVoices()
        {
            var availableVoices = new SpeechSynthesizer().GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => v.VoiceInfo.Name)
                .ToList();

            Log.Debug(string.Format("GetAvailableVoices returing {0} voices", availableVoices.Count));

            return availableVoices;
        }

        public void PlaySound(string soundLocation)
        {
            if (string.IsNullOrEmpty(soundLocation)) return;

            try
            {
                Log.Debug(string.Format("Playing sound '{0}'", soundLocation));

                var player = new System.Media.SoundPlayer(soundLocation);
                player.Play();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
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
    }
}
