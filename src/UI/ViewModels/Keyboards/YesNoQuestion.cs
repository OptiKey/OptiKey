using System;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class YesNoQuestion : BindableBase, IKeyboard
    {
        private readonly string text;
        private readonly Action yesAction;
        private readonly Action noAction;

        public YesNoQuestion(
            string text,
            Action yesAction,
            Action noAction)
        {
            this.text = text;
            this.yesAction = yesAction;
            this.noAction = noAction;
        }
        
        public string Text { get { return text; } }
        public Action YesAction { get { return yesAction; } }
        public Action NoAction { get { return noAction; } }
    }
}
