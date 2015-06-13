using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Size : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Size(IKeyboard back)
        {
            this.back = back;
        }
        
        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
