namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public abstract class Keyboard : IKeyboard
    {
        protected Keyboard(bool simulateKeystrokesSupported = true)
        {
            SimulateKeystrokesSupported = simulateKeystrokesSupported;
        }

        public bool SimulateKeystrokesSupported { get; private set; }
    }
}
