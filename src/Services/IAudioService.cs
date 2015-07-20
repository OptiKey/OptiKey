using System;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IAudioService : INotifyErrors
    {
        void CancelSpeech();
        void Speak(string textToSpeak, Action callBack, int? volume = null, int? rate = null, string voice = null);
        List<string> GetAvailableVoices();
        void PlaySound(string soundLocation, int volume);
    }
}
