// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Voice : BackActionKeyboard
    {
        private const int VoiceKeyRows = 4;
        private const int VoiceKeyColumns = 6;
        private const int VoiceKeyCount = VoiceKeyRows * VoiceKeyColumns;

        private readonly VoiceKey[] voiceKeys = new VoiceKey[VoiceKeyCount];
        private readonly List<string> remainingVoices;

        public Voice(Action backAction, List<string> voices) : base(backAction)
        {
            int displayedVoiceCount = (voices.Count < VoiceKeyCount)
                ? voices.Count // We have enough room to display keys for all the voices as well as a key for "Back".
                : VoiceKeyCount - 2; // Display as much as we can. Reserve a key for "More" in addition to "Back".

            remainingVoices = voices.Skip(displayedVoiceCount).ToList();

            for (int i = 0; i < displayedVoiceCount; i++)
            {
                voiceKeys[i] = new VoiceKey
                {
                    Text = voices[i],
                    KeyValue = new KeyValue(FunctionKeys.SelectVoice, voices[i]),
                };
            }

            RecreateBackAndMoreKeys();
        }

        public int RowCount { get { return VoiceKeyRows; } }
        public int ColumnCount { get { return VoiceKeyColumns; } }

        public IEnumerable<VoiceKey> VoiceKeys { get { return voiceKeys; } }

        public List<string> RemainingVoices { get { return remainingVoices; } }

        public void LocalizeKeys()
        {
            RecreateBackAndMoreKeys();
        }

        private void RecreateBackAndMoreKeys()
        {
            voiceKeys[VoiceKeyCount - 1] = new VoiceKey
            {
                Text = Resources.BACK,
                KeyValue = KeyValues.BackFromKeyboardKey,
            };

            if (remainingVoices.Any())
            {
                voiceKeys[VoiceKeyCount - 2] = new VoiceKey
                {
                    Text = Resources.MORE,
                    KeyValue = KeyValues.MoreKey,
                };
            }
        }
    }

    public struct VoiceKey
    {
        public string Text { get; set; }
        public KeyValue KeyValue { get; set; }
    }
}
