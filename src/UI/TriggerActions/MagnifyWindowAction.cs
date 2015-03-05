using System;
using System.Windows;
using System.Windows.Interactivity;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.Windows;
using log4net;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.UI.TriggerActions
{
    public class MagnifyWindowAction : TriggerAction<FrameworkElement>
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                var magnificationArgs = args.Context as NotificationWithMagnificationArgs;

                var childWindow = new MagnifyWindow(magnificationArgs.Point, magnificationArgs.HorizontalPixels, 
                    magnificationArgs.VerticalPixels, magnificationArgs.OnSelectionAction);

                //Size the window to fill the configured horizontal/vertical percentages of the screen
                var windowStateService = new WindowStateService(childWindow, null, null, null, null, null, null, null, null, null, null, null);
                windowStateService.FillPercentageOfScreen(magnificationArgs.FillHorizontalPercentageOfScreen, magnificationArgs.FillVerticalPercentageOfScreen);
                
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
                bool parentWindowWasTopmost = false;
                if (parentWindow != null)
                {
                    childWindow.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                    parentWindowHadFocus = parentWindow.IsFocused;
                    parentWindowWasTopmost = parentWindow.Topmost;
                    parentWindow.Topmost = false; //Topmost must be revoked otherwise it cannot be reinstated correctly once the child window is closed
                }

                Log.Debug("Showing Magnify window");
                childWindow.ShowDialog();
    
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
