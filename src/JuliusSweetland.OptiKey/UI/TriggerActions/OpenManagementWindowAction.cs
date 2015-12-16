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
                var notificationWithServices = args.Context as NotificationWithServices;

                if (notificationWithServices == null
                    || notificationWithServices.AudioService == null
                    || notificationWithServices.DictionaryService == null)
                {
                    throw new ApplicationException(Resources.REQUIRED_SERVICES_NOT_PASSED_TO_MANAGEMENT_WINDOW);
                }

                var childWindow = new ManagementWindow(notificationWithServices.AudioService, notificationWithServices.DictionaryService);
                
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
                if (parentWindow != null)
                {
                    childWindow.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                    parentWindowHadFocus = parentWindow.IsFocused;
                }

                Log.Info("Showing Management window");
                childWindow.ShowDialog();
    
                if (parentWindow != null)
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
