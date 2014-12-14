namespace JuliusSweetland.ETTA.UI.ViewModels.Keyboards
{
    public class SettingCategories : IKeyboard, INavigableKeyboard
    {
        private readonly IKeyboard back;

        public SettingCategories(IKeyboard back)
        {
            this.back = back;
        }

        public IKeyboard Back
        {
            get { return back; }
        }
    }
}
