using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IAudioService : INotifyErrors
    {
        void Speak(string textToSpeak, int? volume, int? rate, string voice);
        List<string> GetAvailableVoices();
        void PlaySound(string soundLocation, int volume);
    }
}
