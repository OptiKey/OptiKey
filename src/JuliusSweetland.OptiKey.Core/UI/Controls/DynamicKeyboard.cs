// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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

        private bool persistNewState;
        private string windowState;
        private string position;
        private string dockSize;
        private string width;
        private string height;
        private string horizontalOffset;
        private string verticalOffset;
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

        public void OverrideKeyboardLayout(bool persistNewState, string windowState, string position, string dockSize, string width, string height, string horizontalOffset, string verticalOffset)
        {
            this.persistNewState = persistNewState;
            this.windowState = windowState;
            this.position = position;
            this.dockSize = dockSize;
            this.width = width;
            this.height = height;
            this.horizontalOffset = horizontalOffset;
            this.verticalOffset = verticalOffset;
        }

        private void SetupKeyboardLayout()
        {
            if (!string.IsNullOrWhiteSpace(windowState)
                || !string.IsNullOrWhiteSpace(position)
                || !string.IsNullOrWhiteSpace(dockSize)
                || !string.IsNullOrWhiteSpace(width)
                || !string.IsNullOrWhiteSpace(height)
                || !string.IsNullOrWhiteSpace(horizontalOffset)
                || !string.IsNullOrWhiteSpace(verticalOffset))
            {
                string errorMessage = null;
                double validNumber;
                WindowStates validWindowState;
                MoveToDirections validPosition;
                DockSizes validDockSize;
                if (!string.IsNullOrWhiteSpace(windowState) && !Enum.TryParse<WindowStates>(windowState, out validWindowState))
                    errorMessage = "WindowState not valid";
                else if (!string.IsNullOrWhiteSpace(position) && !Enum.TryParse<MoveToDirections>(position, out validPosition))
                    errorMessage = "Position not valid";
                else if (!string.IsNullOrWhiteSpace(dockSize) && !Enum.TryParse<DockSizes>(dockSize, out validDockSize))
                    errorMessage = "DockSize not valid";
                else if (!string.IsNullOrWhiteSpace(width)
                    && !(double.TryParse(width.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                    errorMessage = "Width must be between -9999 and 9999";
                else if (!string.IsNullOrWhiteSpace(height)
                    && !(double.TryParse(height.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                    errorMessage = "Height must be between -9999 and 9999";
                else if (!string.IsNullOrWhiteSpace(horizontalOffset)
                    && !(double.TryParse(horizontalOffset.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                    errorMessage = "Offset must be between -9999 and 9999";
                else if (!string.IsNullOrWhiteSpace(verticalOffset)
                    && !(double.TryParse(verticalOffset.Replace("%", ""), out validNumber) && validNumber >= -9999 && validNumber <= 9999))
                    errorMessage = "Offset must be between -9999 and 9999";

                if (errorMessage != null)
                {
                    raiseToastNotification(Resources.DYNAMIC_KEYBOARD_DEFINITION_INVALID, errorMessage,
                        NotificationTypes.Error, () => { });
                    return;
                }
            }

            Log.InfoFormat("Overriding size and position for dynamic keyboard");
            windowManipulationService.OverrideSizeAndPosition(persistNewState, windowState, position, dockSize, width, height, horizontalOffset, verticalOffset);
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