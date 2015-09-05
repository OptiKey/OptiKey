using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Menu : BackActionKeyboard
    {
        public Menu(Action backAction) : base(backAction)
        {
        }
    }
}
