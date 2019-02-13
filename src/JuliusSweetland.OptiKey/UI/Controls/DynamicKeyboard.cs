using System;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Enums;
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

        private readonly IWindowManipulationService windowManipulationService;
        private readonly IKeyStateService keyStateService;
        private readonly IInputService inputService;
        private readonly IAudioService audioService;
        private readonly Func<string, string, NotificationTypes, Action, bool> raiseToastNotification;

        public DynamicKeyboard(Action backAction,
            IWindowManipulationService windowManipulationService, 
            IKeyStateService keyStateService, 
            IInputService inputService,
            IAudioService audioService,
            Func<string, string, NotificationTypes, Action, bool> raiseToastNotification,
            string link) : base(backAction)
        {
            this.windowManipulationService = windowManipulationService;
            this.link = link;
            this.keyStateService = keyStateService;
            this.inputService = inputService;
            this.audioService = audioService;
            this.raiseToastNotification = raiseToastNotification;
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
                if (overrideHeight.Value > 95)
                {
                    raiseToastNotification(Resources.DYNAMIC_KEYBOARD_DEFINITION_INVALID, Resources.DYNAMIC_KEYBOARD_HEIGHT_ABOVE_THRESHOLD, 
                        NotificationTypes.Error, () => { });
                    return;
                }
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

        private void RaiseError(string title, string message, NotificationTypes notificationType)
        {
            Log.Error("Error raised by dynamic keyboard. Raising ErrorNotificationRequest and playing ErrorSoundFile (from settings)");

            inputService.RequestSuspend();

            if (raiseToastNotification(title, message, notificationType, () => inputService.RequestResume()))
            {
                if (notificationType == NotificationTypes.Error)
                {
                    audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                }
                else
                {
                    audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                }
            }
        }
    }
}
