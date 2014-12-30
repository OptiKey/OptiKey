using System;
using System.Collections.Generic;

namespace JuliusSweetland.ETTA.Services
{
    public interface IAudioService : INotifyErrors
    {
        void Speak(string textToSpeak, int? volume, int? rate, string voice);
        List<string> GetAvailableVoices();
        void PlaySound(string soundLocation);
    }
}
