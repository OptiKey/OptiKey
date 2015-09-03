using System.Collections.Generic;
using System.Reflection;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Services;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    internal class KeyStateServiceState
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly KeyStateService keyStateService;
        private readonly Dictionary<KeyValue, KeyDownStates> keyDownStates = new Dictionary<KeyValue, KeyDownStates>();

        public KeyStateServiceState(KeyStateService keyStateService)
        {
            Log.Debug("Saving state.");
            this.keyStateService = keyStateService;
            foreach (var key in KeyValues.KeysWhichCanBePressedOrLockedDown)
            {
                Log.DebugFormat("Storing key down state of '{0}' as '{1}'.", key, keyDownStates[key]);
                keyDownStates[key] = keyStateService.KeyDownStates[key].Value;
            }
        }

        public void RestoreState()
        {
            Log.Debug("Restoring state.");
            foreach (var key in keyDownStates.Keys)
            {
                Log.DebugFormat("Restoring key down state on '{0}' to '{1}'.", key, keyDownStates[key]);
                keyStateService.KeyDownStates[key].Value = keyDownStates[key];
            }
        }
    }
}
