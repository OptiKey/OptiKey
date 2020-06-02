// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Mouse.Properties
{

    class Settings : JuliusSweetland.OptiKey.Properties.Settings
    {

        public static void Initialise()
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
            InitialiseWithDerivedSettings(defaultInstance);
        }

        public override AppType GetApp()
        {
            return AppType.Mouse;
        }

        #region App-specific settings
        
        /*
         * Override the settings relating to mouse mode, to lock user into
         * mouse mode only.
         */

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool MouseOnlyMode
        {
            get
            {
                return true;
            }
            set
            {
                // no-op, can't be changed in this app
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Mouse")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override global::JuliusSweetland.OptiKey.Enums.Keyboards StartupKeyboard
        {
            get
            {
                return Enums.Keyboards.Mouse;
            }
            set
            {
                // no-op, can't be changed in this app
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool AllowMultipleInstances
        {
            get
            {
                return false;
            }
            set
            {
                // no-op, can't be changed in this app
            }
        }

        #endregion


        #region Unsupported features

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool EnableCommuniKateKeyboardLayout
        {
            get
            {
                return false;
            }
            set
            {
                // no-op
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool ConversationOnlyMode
        {
            get
            {
                return false;
            }
            set
            {
                // no-op, can't be changed in this app
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool EnableAttentionKey
        {
            get { return false; }
            set
            {
                // no-op, can't be changed in this app
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool ConversationConfirmEnable
        {
            get { return false; }
            set { /* no-op */ }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool EnableQuitKeys
        {
            get { return false; }
            set { /* no-op */ }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool EnableCopyAllScratchpadKey
        {
            get { return false; }
            set { /* no-op */ }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool EnableTranslationKey
        {
            get { return false; }
            set { /* no-op */ }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public override bool MultiKeySelectionEnabled
        {
            get { return false; }
            set { /* no-op */ }
        }

        #endregion

    }
}
