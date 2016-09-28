using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class CustomKeyboardSelector : BackActionKeyboard
    {
        private int pageIndex;

        public CustomKeyboardSelector(Action backAction, int pageIndex)
            : base(backAction)
        {
            this.pageIndex = pageIndex;
        }

        public int PageIndex
        {
            get { return pageIndex; }
        }
    }
}
