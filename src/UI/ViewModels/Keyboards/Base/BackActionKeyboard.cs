using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public abstract class BackActionKeyboard : Keyboard, IBackAction
    {
        private readonly Action backAction;

        protected BackActionKeyboard(Action backAction, bool simulateKeystrokesSupported = true)
            : base(simulateKeystrokesSupported)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
