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

        private readonly bool simulateKeyStrokes;
        private readonly KeyStateService keyStateService;
        private readonly Dictionary<KeyValue, KeyDownStates> keyDownStates = new Dictionary<KeyValue, KeyDownStates>();

        public KeyStateServiceState(bool simulateKeyStrokes, KeyStateService keyStateService)
        {
            Log.DebugFormat("Saving KeyStateService state for SimulateKeyStrokes={0}.", simulateKeyStrokes);
            this.simulateKeyStrokes = simulateKeyStrokes;
            this.keyStateService = keyStateService;
            foreach (var key in keyStateService.KeyDownStates.Keys)
            {
                Log.DebugFormat("Storing key down state of '{0}' as '{1}'.", key, keyStateService.KeyDownStates[key]);
                keyDownStates[key] = keyStateService.KeyDownStates[key].Value;
            }
        }

        public void RestoreState()
        {
            Log.DebugFormat("Restoring KeyStateService state for SimulateKeyStrokes={0}.", simulateKeyStrokes);
            foreach (var key in keyDownStates.Keys)
            {
                Log.DebugFormat("Restoring key down state on '{0}' to '{1}'.", key, keyDownStates[key]);
                keyStateService.KeyDownStates[key].Value = keyDownStates[key];
            }
        }
    }
}
