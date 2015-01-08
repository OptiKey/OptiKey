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
        
        public InteractionRequest<Confirmation> ConfirmationRequest { get; private set; }
        public DelegateCommand<Window> OkCommand { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        
        #endregion
        
        #region Load / Save Settings

        private void LoadSettings()
        {
            
        }

        private void SaveSettings()
        {
            
            Settings.Default.Save();
        }
        
        #endregion
        
        #region Ok / Cancel

        private void Ok(Window window)
        {
            //Check which settings have been changed
            bool restartRequired = true;//Settings.Default.CaptureTriggerSource != CaptureTriggerSource
            //                       || Settings.Default.CaptureTriggerKeyboardSignal != CaptureTriggerKeyboardSignal.ToString()
            //                       || Settings.Default.CaptureCoordinatesSource != CaptureCoordinatesSource
            //                       || Settings.Default.CaptureMouseCoordinatesOnIntervalInMilliseconds != CaptureMouseCoordinatesOnIntervalInMilliseconds
            //                       || Settings.Default.CaptureCoordinatesTimeoutInMilliseconds != CaptureCoordinatesTimeoutInMilliseconds;
            
            //Warn if restart required and prompt for Confirmation before restarting
            if (restartRequired)
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = "May I restart?",
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

        #endregion
    }
}
