// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

namespace JuliusSweetland.OptiKey.Pro.Properties {

    class Settings : JuliusSweetland.OptiKey.Properties.Settings
    {
        
        public static void Initialise()
        {
            Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
            InitialiseWithDerivedSettings(defaultInstance);            
        }
    }
}
