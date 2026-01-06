// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base
{
    public abstract class Keyboard : IKeyboard
    {
        protected Keyboard(bool simulateKeyStrokes = true, bool multiKeySelectionSupported = false)
        {
            SimulateKeyStrokes = simulateKeyStrokes;
            MultiKeySelectionSupported = multiKeySelectionSupported;
        }

        public bool SimulateKeyStrokes { get; set; }
        public bool MultiKeySelectionSupported { get; private set; }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }        
    }
}
