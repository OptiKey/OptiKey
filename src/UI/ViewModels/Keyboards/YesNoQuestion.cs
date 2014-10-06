using System;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class YesNoQuestion : BindableBase, IKeyboard
    {
        private readonly string text;

        public YesNoQuestion(string text)
        {
            this.text = text;
        }
        
        public string Text { get { return text; } }
        public Action YesAction { get; set; }
        public Action NoAction { get; set; }
    }
}
