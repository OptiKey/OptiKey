using JuliusSweetland.OptiKey.Services;

namespace JuliusSweetland.OptiKey.Models
{
    internal class KeyStateServiceState
    {
        private readonly KeyStateService keyStateService;

        public KeyStateServiceState(KeyStateService keyStateService)
        {
            this.keyStateService = keyStateService;
            //TODO: Store the state from keyStateService
            /*
             * NotifyingConcurrentDictionary<KeyValue, KeyDownStates> KeyDownStates
             * KeyEnabledStates KeyEnabledStates 
             */
        }

        public void RestoreState()
        {
            //TODO: Restore state onto keyStateService
        }
    }
}
