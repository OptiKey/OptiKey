using System;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public abstract class BackActionKeyboard : Keyboard, IBackAction
    {
        private readonly Action backAction;

        protected BackActionKeyboard(Action backAction, bool simulateKeyStrokes = true, bool multiKeySelectionSupported = false)
            : base(simulateKeyStrokes, multiKeySelectionSupported)
        {
            this.backAction = backAction;
        }

        public Action BackAction
        {
            get { return backAction; }
        }
    }
}
