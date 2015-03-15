using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class SizeAndOpacity : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public SizeAndOpacity(IKeyboard back)
        {
            this.back = back;
        }
        
        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
