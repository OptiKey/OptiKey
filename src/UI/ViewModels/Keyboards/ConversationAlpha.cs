using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class ConversationAlpha : BackActionKeyboard
    {
        public ConversationAlpha(Action backAction) : base(backAction, simulateKeystrokesSupported:false)
        {
        }
    }
}
