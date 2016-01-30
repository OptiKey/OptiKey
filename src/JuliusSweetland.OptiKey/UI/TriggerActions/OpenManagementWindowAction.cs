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
        
        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                var notificationWithServicesAndState = args.Context as NotificationWithServicesAndState;

                if (notificationWithServicesAndState == null
                    || notificationWithServicesAndState.AudioService == null
                    || notificationWithServicesAndState.DictionaryService == null)
                {
                    throw new ApplicationException(Resources.REQUIRED_SERVICES_NOT_PASSED_TO_MANAGEMENT_WINDOW);
                }

                var childWindow = new ManagementWindow(notificationWithServicesAndState.AudioService, notificationWithServicesAndState.DictionaryService);
                
                EventHandler closeHandler = null;
                closeHandler = (sender, e) =>
                {
                    childWindow.Closed -= closeHandler;
                    args.Callback();
                };
                childWindow.Closed += closeHandler;

                var parentWindow = AssociatedObject != null
                    ? AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject)
                    : null;

                bool parentWindowHadFocus = false;
                if (parentWindow != null
                    && notificationWithServicesAndState.ModalWindow)
                {
                    childWindow.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                    parentWindowHadFocus = parentWindow.IsFocused;
                }

                if (notificationWithServicesAndState.ModalWindow)
                {
                    Log.Info("Showing Management Window (modal)");
                    childWindow.ShowDialog();
                }
                else
                {
                    Log.Info("Showing Management Window (non-modal)");
                    childWindow.Show();
                }
    
                if (parentWindow != null
                    && notificationWithServicesAndState.ModalWindow)
                {
                    if(parentWindowHadFocus)
                    {
                        Log.Debug("Parent Window was previously focussed - giving it focus again.");
                        parentWindow.Focus();
                    }
                }
            }
        }
    }
}
