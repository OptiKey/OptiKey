using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Properties;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class AudioService : IAudioService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int? currentWaveOutVolume; //Volumes are scaled from 0 to 100

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public AudioService()
        {
            uint tempWavOutVol = 0;
            PInvoke.waveOutGetVolume(IntPtr.Zero, out tempWavOutVol);
            var vol = (ushort)(tempWavOutVol & 0x0000ffff);
            currentWaveOutVolume = vol / (ushort.MaxValue / 100);
        }

        #endregion

        #region Methods

        public void Speak(string textToSpeak, int? volume = null, int? rate = null, string voice = null)
        {
            if (string.IsNullOrEmpty(textToSpeak)) return;

            try
            {
                Log.DebugFormat("Speaking '{0}' with volume '{1}', rate '{2}' and voice '{3}'", 
                    textToSpeak, volume, rate, voice);

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

            Log.DebugFormat("GetAvailableVoices returing {0} voices", availableVoices.Count);

            return availableVoices;
        }

        public void PlaySound(string soundLocation, int volume)
        {
            if (string.IsNullOrEmpty(soundLocation)) return;

            try
            {
                Log.DebugFormat("Playing sound '{0}' at volume", soundLocation, volume);

                try
                {
                    if (currentWaveOutVolume == null || currentWaveOutVolume.Value != volume)
                    {
                        int newVolume = ((ushort.MaxValue / 100) * volume);
                        uint newVolumeAllChannels = (((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16)); //Set the volume on left and right channels
                        PInvoke.waveOutSetVolume(IntPtr.Zero, newVolumeAllChannels);
                        currentWaveOutVolume = volume;
                    }
                }
                catch (Exception exception)
                {
                    var customException = new ApplicationException(
                        string.Format("There was a problem setting the wave out volume to '{0}'", volume), exception);
                    
                    //Don't publish error - the error handler tries to play a sound file which could loop us right back here
                    Log.Error("Exception encountered within the AudioService", customException);
                }

                var player = new System.Media.SoundPlayer(soundLocation);
                player.Play();
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
    }
}
