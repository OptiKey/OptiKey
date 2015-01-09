using System.Windows;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using log4net;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels
{
    public class ManagementViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionaryService dictionaryService;

        #endregion
        
        #region Ctor

        public ManagementViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;

            ConfirmationRequest = new InteractionRequest<Confirmation>();
            OkCommand = new DelegateCommand<Window>(Ok); //Can always click Ok
            CancelCommand = new DelegateCommand<Window>(Cancel); //Can always click Cancel
            
            LoadSettings();
        }
        
        #endregion
        
        #region Properties
        
        private bool debug;
        public bool Debug
        {
            get { return debug; }
            set { SetProperty(ref debug, value); }
        }
        
        public InteractionRequest<Confirmation> ConfirmationRequest { get; private set; }
        public DelegateCommand<Window> OkCommand { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        
        #endregion
        
        #region Methods

        private void LoadSettings()
        {
            Debug = Settings.Default.Debug;
        }

        private void SaveSettings()
        {
            Settings.Default.Debug = Debug;
            
            Settings.Default.Save();
        }

        private void Ok(Window window)
        {
            //Warn if restart required and prompt for Confirmation before restarting
            if (ChangesRequireRestart())
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = "May I restart ETTA?",
                        Content = "ETTA needs to restart to apply your changes.\nPlease click OK to continue with the restart, or CANCEL to discard your changes"
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            SaveSettings();
                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                    });
            }
            else
            {
                //if (Settings.Default.Language != Language)
                //{
                //    dictionaryService.LoadDictionary(Language);
                //}
                SaveSettings();
                window.Close();
            }
        }

        private void Cancel(Window window)
        {
            window.Close();
        }
        
        private bool ChangesRequireRestart()
        {
            return false;
            
            //Settings.Default.CaptureTriggerSource != CaptureTriggerSource
            //  || Settings.Default.CaptureTriggerKeyboardSignal != CaptureTriggerKeyboardSignal.ToString()
            //  || Settings.Default.CaptureCoordinatesSource != CaptureCoordinatesSource
            //  || Settings.Default.CaptureMouseCoordinatesOnIntervalInMilliseconds != CaptureMouseCoordinatesOnIntervalInMilliseconds
            //  || Settings.Default.CaptureCoordinatesTimeoutInMilliseconds != CaptureCoordinatesTimeoutInMilliseconds;
        }

        #endregion
    }
}
