namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public abstract class Keyboard : IKeyboard
    {
        protected Keyboard(bool simulateKeyStrokes = true)
        {
            SimulateKeyStrokes = simulateKeyStrokes;
        }

        public bool SimulateKeyStrokes { get; private set; }
    }
}
