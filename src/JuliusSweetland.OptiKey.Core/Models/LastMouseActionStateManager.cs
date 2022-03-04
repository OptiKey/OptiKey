// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class LastMouseActionStateManager : BindableBase, ILastMouseActionStateManager
    {
        private Action lastMouseAction;
        private bool lastMouseActionExists;

        public Action LastMouseAction
        {
            get { return lastMouseAction; }
            set
            {
                lastMouseAction = value;
                LastMouseActionExists = lastMouseAction != null;
            }
        }

        public bool LastMouseActionExists
        {
            get { return lastMouseActionExists; }
            private set { SetProperty(ref lastMouseActionExists, value); }
        }
    }
}
