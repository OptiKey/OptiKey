using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Mouse : BackActionKeyboard
    {
        public Mouse() : this(() => { }) //Default constructor required to create an instance of this in the MouseWindow resources
        {
        }

        public Mouse(Action backAction) : base(backAction)
        {
        }
    }
}
