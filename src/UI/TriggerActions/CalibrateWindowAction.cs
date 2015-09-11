using System;
using System.Windows;
using System.Windows.Interactivity;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.Utilities;
using log4net;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.UI.TriggerActions
{
    public class CalibrateWindowAction : TriggerAction<FrameworkElement>
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Invoke

        protected async override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;

            if (args == null)
            {
                return;
            }

            if (CalibrationService == null)
            {
                Log.Error("CalibrateWindowAction was invoked, but the CalibrationService (dependency property) is not set. Calibration is not possible.");
                return;
            }

            Window parentWindow = null;
            bool parentWindowHadFocus = false;
            if (AssociatedObject != null)
            {
                parentWindow = AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject);
                if (parentWindow != null)
                {
                    parentWindowHadFocus = parentWindow.IsFocused;
                }
            }

            var calibrationResult = args.Context as NotificationWithCalibrationResult;

            try
            {
                var message = await CalibrationService.Calibrate(parentWindow);
                if (calibrationResult != null)
                {
                    calibrationResult.Success = true;
                    calibrationResult.Message = message;
                }
            }
            catch (Exception exception)
            {
                if (calibrationResult != null)
                {
                    calibrationResult.Success = false;
                    calibrationResult.Exception = exception;
                }
            }
            
            args.Callback();

            if (parentWindow != null
                && parentWindowHadFocus)
            {
                Log.Debug("Parent Window was previously focussed - giving it focus again.");
                parentWindow.Focus();
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CalibrationServiceProperty =
            DependencyProperty.Register("CalibrationService", typeof (ICalibrationService), typeof (CalibrateWindowAction), new PropertyMetadata(default(ICalibrationService)));

        public ICalibrationService CalibrationService
        {
            get { return (ICalibrationService) GetValue(CalibrationServiceProperty); }
            set { SetValue(CalibrationServiceProperty, value); }
        }

        #endregion
    }
}
