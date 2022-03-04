// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using System.Windows.Interactivity;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.Windows;
using log4net;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.UI.TriggerActions
{
    public class ConfirmationWindowAction : TriggerAction<FrameworkElement>
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Log.Info("Opening confirmation window");
                var childWindow = new ConfirmationWindow { DataContext = args.Context };

                EventHandler closeHandler = null;
                closeHandler = (sender, e) =>
                {
                    childWindow.Closed -= closeHandler;
                    args.Callback();
                };
                childWindow.Closed += closeHandler;

                childWindow.Owner = AssociatedObject != null
                    ? AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject)
                    : childWindow.Owner;

                childWindow.ShowDialog();
            }
        }
    }
}
