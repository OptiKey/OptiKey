namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class Menu : IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Menu(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
