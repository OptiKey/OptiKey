using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class Move : BindableBase, IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;
        private int adjustmentAmountInPixels;

        public Move(int adjustmentAmountInPixels, IKeyboard back)
        {
            this.adjustmentAmountInPixels = adjustmentAmountInPixels;
            this.back = back;
        }

        public int AdjustmentAmountInPixels
        {
            get { return adjustmentAmountInPixels; }
            set { SetProperty(ref adjustmentAmountInPixels, value); }
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
