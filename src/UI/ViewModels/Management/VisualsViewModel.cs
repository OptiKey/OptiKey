using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class VisualsViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public VisualsViewModel()
        {
            LoadSettings();
        }
        
        #endregion
        
        #region Properties

        public bool ChangesRequireRestart
        {
            get
            {
                return false;

                //Settings.Default.CaptureTriggerSource != CaptureTriggerSource
                //  || Settings.Default.CaptureTriggerKeyboardSignal != CaptureTriggerKeyboardSignal.ToString()
                //  || Settings.Default.CaptureCoordinatesSource != CaptureCoordinatesSource
                //  || Settings.Default.CaptureMouseCoordinatesOnIntervalInMilliseconds != CaptureMouseCoordinatesOnIntervalInMilliseconds
                //  || Settings.Default.CaptureCoordinatesTimeoutInMilliseconds != CaptureCoordinatesTimeoutInMilliseconds;
            }
        }
        
        #endregion
        
        #region Methods

        private void LoadSettings()
        {
            //Debug = Settings.Default.Debug;
        }

        private void SaveSettings()
        {
            //Settings.Default.Debug = Debug;
            
            //Settings.Default.Save();
        }

        #endregion
    }
}
