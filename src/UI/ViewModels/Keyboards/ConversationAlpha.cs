using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class ConversationAlpha : BackActionKeyboard
    {
        public ConversationAlpha(Action backAction) : base(backAction, simulateKeyStrokes: false)
        {
        }
    }
}
