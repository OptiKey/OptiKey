﻿// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

namespace JuliusSweetland.OptiKey.Mouse.Properties
{

    class Settings : JuliusSweetland.OptiKey.Properties.Settings
    {

        public static void Initialise()
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
            InitialiseWithDerivedSettings(defaultInstance);
        }

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
        public virtual global::JuliusSweetland.OptiKey.Enums.Keyboards StartupKeyboard
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
    }
}