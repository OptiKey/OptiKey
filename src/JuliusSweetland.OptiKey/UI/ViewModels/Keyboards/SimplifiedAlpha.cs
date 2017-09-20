using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class SimplifiedAlpha : BackActionKeyboard, IConversationKeyboard
    {
        public SimplifiedAlpha(Action backAction)
            : base(backAction, simulateKeyStrokes: false, multiKeySelectionSupported: false)
        {
        }
    }
}
