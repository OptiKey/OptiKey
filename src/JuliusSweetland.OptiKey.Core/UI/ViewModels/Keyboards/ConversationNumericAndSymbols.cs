// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class ConversationNumericAndSymbols : BackActionKeyboard, IConversationKeyboard
    {
        public ConversationNumericAndSymbols(Action backAction) : base(backAction, simulateKeyStrokes: false)
        {
        }
    }
}
