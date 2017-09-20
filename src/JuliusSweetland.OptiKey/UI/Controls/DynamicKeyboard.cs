using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class DynamicKeyboard : BackActionKeyboard
    {
        private string link;
        private Action<double> resizeAction;

        public DynamicKeyboard(Action backAction, Action<double> resizeAction, string link)
            : base(backAction)
        {
            this.link = link;
            this.resizeAction = resizeAction;
        }

        public string Link
        {
            get { return link; }
        }

        public Action<double> ResizeAction
        {
            get { return resizeAction; }
        }
    }
}
