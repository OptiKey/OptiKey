using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class WordsViewModel : BindableBase
    {
        private readonly IDictionaryService dictionaryService;

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public WordsViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;

            LoadSettings();
        }

        #endregion
        
        #region Properties
        
        private Languages language;
        public Languages Language
        {
            get { return language; }
            set { SetProperty(ref this.language, value); }
        }

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
            Language = Settings.Default.Language;
        }

        public void ApplyChanges()
        {
            bool reloadDictionary = Settings.Default.Language != Language;

            Settings.Default.Language = Language;
            Settings.Default.Save();

            if (reloadDictionary)
            {
                dictionaryService.LoadDictionary();
            }
        }

        #endregion
    }
}
