using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class PositionAndOpacity : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public PositionAndOpacity(IKeyboard back)
        {
            this.back = back;
        }
        
        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
