using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class SimplifiedConversationAlpha : BackActionKeyboard, IConversationKeyboard
    {
        public SimplifiedConversationAlpha(Action backAction)
            : base(backAction, simulateKeyStrokes: false, multiKeySelectionSupported: false)
        {
        }
    }
}
