// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.UnitTests.Properties {

    class Settings : JuliusSweetland.OptiKey.Properties.Settings
    {
        
        public static void Initialise()
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
            InitialiseWithDerivedSettings(defaultInstance);            
        }

        public override AppType GetApp()
        {
            return AppType.Tests;
        }
    }
}
