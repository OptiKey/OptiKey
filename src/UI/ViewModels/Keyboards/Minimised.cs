using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Minimised : IKeyboard, IBackAction
    {
        private readonly Action backAction;

        public Minimised(Action backAction)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
