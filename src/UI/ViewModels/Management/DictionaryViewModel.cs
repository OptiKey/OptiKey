using System.Windows;
using JuliusSweetland.ETTA.Properties;
using log4net;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class DictionaryViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionaryService dictionaryService;
        
        #endregion
        
        #region Ctor

        public DictionaryViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;
        
            LoadSettings();
        }
        
        #endregion
        
        #region Properties
        
        public bool ChangesRequireRestart
        {
            return false;
            
            //Settings.Default.CaptureTriggerSource != CaptureTriggerSource
            //  || Settings.Default.CaptureTriggerKeyboardSignal != CaptureTriggerKeyboardSignal.ToString()
            //  || Settings.Default.CaptureCoordinatesSource != CaptureCoordinatesSource
            //  || Settings.Default.CaptureMouseCoordinatesOnIntervalInMilliseconds != CaptureMouseCoordinatesOnIntervalInMilliseconds
            //  || Settings.Default.CaptureCoordinatesTimeoutInMilliseconds != CaptureCoordinatesTimeoutInMilliseconds;
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
