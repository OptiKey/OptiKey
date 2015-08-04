using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public abstract class BackActionKeyboard : IKeyboard, IBackAction
    {
        private readonly Action backAction;

        public BackActionKeyboard(Action backAction)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
