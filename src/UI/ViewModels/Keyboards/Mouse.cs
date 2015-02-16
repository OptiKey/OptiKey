namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Mouse : IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public Mouse(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
