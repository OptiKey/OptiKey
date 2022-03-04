// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using System.Windows.Interactivity;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.Windows;
using log4net;
using Prism.Interactivity.InteractionRequest;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.TriggerActions
{
    public class OpenManagementWindowAction : TriggerAction<FrameworkElement>
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ManagementWindow managementWindow;

        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                var notificationWithServicesAndState = args.Context as NotificationWithServicesAndState;

                if (notificationWithServicesAndState == null
                    || notificationWithServicesAndState.AudioService == null
                    || notificationWithServicesAndState.DictionaryService == null
                    || notificationWithServicesAndState.WindowManipulationService == null)
                {
                    throw new ApplicationException(Resources.REQUIRED_SERVICES_NOT_PASSED_TO_MANAGEMENT_WINDOW);
                }

                // Open new management console, or re-focus existing one
                if (managementWindow == null)
                {
                    managementWindow = new ManagementWindow(notificationWithServicesAndState.AudioService,
                        notificationWithServicesAndState.DictionaryService,
                        notificationWithServicesAndState.WindowManipulationService);

                    EventHandler closeHandler = null;
                    closeHandler = (sender, e) =>
                    {
                        managementWindow.Closed -= closeHandler;
                        args.Callback();
                        managementWindow = null;
                    };
                    managementWindow.Closed += closeHandler;

                    var parentWindow = AssociatedObject != null
                        ? AssociatedObject as Window ??
                          VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject)
                        : null;

                    bool parentWindowHadFocus = false;
                    if (parentWindow != null
                        && notificationWithServicesAndState.ModalWindow)
                    {
                        managementWindow.Owner =
                            parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                        parentWindowHadFocus = parentWindow.IsFocused;
                    }

                    if (notificationWithServicesAndState.ModalWindow)
                    {
                        Log.Info("Showing Management Window (modal)");
                        managementWindow.Topmost = true;
                        managementWindow.ShowDialog(); //This is blocking
                        if (parentWindowHadFocus)
                        {
                            Log.Debug("Parent Window was previously focussed - giving it focus again.");
                            parentWindow.Focus();
                        }
                    }
                    else
                    {
                        Log.Info("Showing Management Window (non-modal)");
                        managementWindow.Show();
                    }
                }
                else
                {
                    managementWindow.Focus();
                }
            }
        }
    }
}
