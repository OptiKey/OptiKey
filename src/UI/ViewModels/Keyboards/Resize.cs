using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class Resize : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Resize(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
