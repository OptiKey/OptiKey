namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class Resize : IKeyboard, INavigableKeyboard
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
