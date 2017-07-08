using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class BlockyAlpha : BackActionKeyboard, IConversationKeyboard
    {
        public BlockyAlpha(Action backAction)
            : base(backAction, simulateKeyStrokes: false, multiKeySelectionSupported: true)
        {
        }
    }
}
