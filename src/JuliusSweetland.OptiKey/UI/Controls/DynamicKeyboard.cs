using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Services;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class DynamicKeyboard : BackActionKeyboard
    {
        private string link;
        private Action<double> resizeAction;

        private Dictionary<Models.KeyValue, Enums.KeyDownStates> resetKeyStates;

        private IKeyStateService keyStateService;

        public DynamicKeyboard(Action backAction, Action<double> resizeAction,
                               IKeyStateService keyStateService, string link)
            : base(backAction)
        {
            this.link = link;
            this.resizeAction = resizeAction;
            this.keyStateService = keyStateService;
        }

        public void ApplyKeyOverrides(Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates,
                                    Dictionary<Models.KeyValue, Enums.KeyDownStates> resets)
        {
            resetKeyStates = resets;

            backAction += () => { this.ResetOveriddenKeyStates(); };

            // Apply overrides
            foreach (var entry in overrideKeyStates)
                keyStateService.KeyDownStates[entry.Key].Value = entry.Value;
        }

        public string Link
        {
            get { return link; }
        }

        public Action<double> ResizeAction
        {
            get { return resizeAction; }
        }

        public void ResetOveriddenKeyStates()
        {
            if (resetKeyStates != null)
            {
                foreach (var entry in resetKeyStates)
                    keyStateService.KeyDownStates[entry.Key].Value = entry.Value;
            }
        }
    }
}
