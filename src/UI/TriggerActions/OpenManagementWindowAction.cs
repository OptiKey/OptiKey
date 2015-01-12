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

                childWindow.Owner = AssociatedObject != null
                    ? AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject)
                    : childWindow.Owner;

                childWindow.ShowDialog();
            }
        }
    }
}
