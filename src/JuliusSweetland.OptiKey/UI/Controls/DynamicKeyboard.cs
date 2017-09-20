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
        private Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates;

        private IKeyStateService keyStateService;

        public DynamicKeyboard(Action backAction, Action<double> resizeAction,
                               IKeyStateService keyStateService, string link)
            : base(backAction)
        {
            this.link = link;
            this.resizeAction = resizeAction;
            this.keyStateService = keyStateService;
        }

        public override void OnEnter()
        {
            ApplyKeyOverrides();
        }

        public override void OnExit()
        {
            ResetOveriddenKeyStates();
        }

        public void SetKeyOverrides(Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates)
        {
            this.overrideKeyStates = overrideKeyStates;
        }

        public void ApplyKeyOverrides()
        {
            if (overrideKeyStates != null)
            {
                resetKeyStates = new Dictionary<Models.KeyValue, Enums.KeyDownStates>();

                // Remember old state for resetting
                foreach (var entry in overrideKeyStates)
                {
                    resetKeyStates.Add(entry.Key, keyStateService.KeyDownStates[entry.Key].Value);
                }
                // Apply overrides
                foreach (var entry in overrideKeyStates)
                {
                    keyStateService.KeyDownStates[entry.Key].Value = entry.Value;
                }
            }
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
                {
                    keyStateService.KeyDownStates[entry.Key].Value = entry.Value;
                }
            }
        }
    }
}
