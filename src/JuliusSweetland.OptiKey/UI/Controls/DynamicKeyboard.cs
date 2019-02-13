using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Services;
using System.Windows;
using JuliusSweetland.OptiKey.Properties;
using log4net;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class DynamicKeyboard : BackActionKeyboard
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string link;
        private Action<double> resizeAction;

        private double? overrideHeight;
        private double? origHeight;

        private Dictionary<Models.KeyValue, Enums.KeyDownStates> resetKeyStates;
        private Dictionary<Models.KeyValue, Enums.KeyDownStates> overrideKeyStates;

        private IWindowManipulationService windowManipulationService;
        private IKeyStateService keyStateService;

        public DynamicKeyboard(Action backAction,IWindowManipulationService windowManipulationService,
                               IKeyStateService keyStateService, string link)
            : base(backAction)
        {
            this.windowManipulationService = windowManipulationService;
            this.link = link;
            this.keyStateService = keyStateService;
        }

        public override void OnEnter()
        {
            ApplyKeyOverrides();
            SetupKeyboardLayout();
        }

        public override void OnExit()
        {
            ResetOveriddenKeyStates();
            ResetKeyboardLayout();
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

        public void OverrideKeyboardLayout(double? height)
        {
            this.overrideHeight = height;
        }

        private void SetupKeyboardLayout()
        {
            if (overrideHeight.HasValue)
            {
                Log.InfoFormat("Overriding dock height for dynamic keyboard: height = {0}", overrideHeight.GetValueOrDefault());
                origHeight = Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen;
                Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = overrideHeight.GetValueOrDefault();
                windowManipulationService.ResizeDockToFull();
            }
        }

        private void ResetKeyboardLayout()
        {
            if (origHeight.HasValue)
            {
                Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = origHeight.GetValueOrDefault();
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
