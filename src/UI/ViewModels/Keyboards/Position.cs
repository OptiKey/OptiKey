using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Position : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Position(IKeyboard back)
        {
            this.back = back;
        }
        
        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
