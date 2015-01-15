using System;
using System.Windows;
using System.Windows.Interactivity;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.Windows;
using log4net;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.ETTA.UI.TriggerActions
{
    public class OpenManagementWindowAction : TriggerAction<FrameworkElement>
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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
                    throw new ApplicationException("Audio and/or Dictionary service(s) were/was not supplied to the management window action.");
                }

                var childWindow = new ManagementWindow(notificationWithServices.AudioService, notificationWithServices.DictionaryService);
                
                EventHandler closeHandler = null;
                closeHandler = (sender, e) =>
                {
                    childWindow.Closed -= closeHandler;
                    args.Callback();
                };
                childWindow.Closed += closeHandler;

                Window parentWindow = null;
                if (AssociatedObject != null)
                {
                    parentWindow = AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject);
                }

                bool parentWindowHadFocus = false;
                bool parentWindowWasTopmost = false;
                if (parentWindow != null)
                {
                    childWindow.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                    parentWindowHadFocus = parentWindow.IsFocused;
                    parentWindowWasTopmost = parentWindow.Topmost;
                }

                Log.Debug("Showing Management window");

                childWindow.Show();
    
                if (parentWindow != null)
                {
                    if(parentWindowHadFocus)
                    {
                        Log.Debug("Parent Window was previously focussed - giving it focus again.");
                        parentWindow.Focus();
                    }
                    
                    if(parentWindowWasTopmost)
                    {
                        Log.Debug("Parent Window was previously top most - setting it back to top most window.");
                        parentWindow.Topmost = true;
                    }
                }
            }
        }
    }
}
