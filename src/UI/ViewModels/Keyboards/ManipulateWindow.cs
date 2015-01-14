using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class ManipulateWindow : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public ManipulateWindow(IKeyboard back)
        {
            this.back = back;
        }
        
        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
