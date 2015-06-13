using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Menu : IKeyboard, IBackAction
    {
        private readonly Action backAction;

        public Menu(Action backAction)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
