// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Keyboards
{
    public class Menu : BackActionKeyboard
    {
        public Menu(Action backAction) : base(backAction)
        {
        }

        public bool DisplayVoicesWhenChangingKeyboardLanguage
        {
            get
            {
                return Settings.Default.DisplayVoicesWhenChangingKeyboardLanguage;
            }
        }
    }
}
