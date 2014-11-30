namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class More : IKeyboard
    {
        private readonly IKeyboard backToKeyboard;

        public More(IKeyboard backToKeyboard)
        {
            this.backToKeyboard = backToKeyboard;
        }

        public IKeyboard BackToKeyboard
        {
            get { return backToKeyboard; }
        }
    }
}
