namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class MoveAndResize : IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public MoveAndResize(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
