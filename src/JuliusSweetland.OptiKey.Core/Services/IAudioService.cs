// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IAudioService : INotifyErrors
    {
        List<string> GetAvailableVoices();
        List<string> GetAvailableMaryTTSVoices();
        void PlaySound(string file, int volume);
        bool SpeakNewOrInterruptCurrentSpeech(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null);
    }
}
