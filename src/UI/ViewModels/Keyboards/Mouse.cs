using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Mouse : IKeyboard, IBackAction
    {
        private readonly Action backAction;

        public Mouse(Action backAction)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
