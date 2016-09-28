using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class CustomKeyboard : BackActionKeyboard            
    {
        public CustomKeyboard(Action backAction)
            : base(backAction)
        {
        }
    }
}
