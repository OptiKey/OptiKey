namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class Move : IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Move(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
