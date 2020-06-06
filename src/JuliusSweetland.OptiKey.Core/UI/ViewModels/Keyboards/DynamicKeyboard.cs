// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Services;
using log4net;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class DynamicKeyboard : BackActionKeyboard
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string link;

        private Dictionary<Models.KeyValue, Enums.KeyDownStates> resetKeyStates;
        private Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates;
        private readonly IKeyStateService keyStateService;

        public DynamicKeyboard(Action backAction,
            IKeyStateService keyStateService,
            string link,
            Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates = null) : base(backAction)
        {
            this.link = link;
            this.keyStateService = keyStateService;
            this.overrideKeyStates = overrideKeyStates;
        }

        public override void OnEnter()
        {
            ApplyKeyOverrides();
        }

        public override void OnExit()
        {
            ResetOveriddenKeyStates();
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
