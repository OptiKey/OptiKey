namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public interface IKeyboard
    {
        bool SimulateKeyStrokes { get; }
        bool MultiKeySelectionSupported { get; }
        void OnEnter();
        void OnExit();
    }
}
