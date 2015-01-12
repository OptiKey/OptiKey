using System.Windows;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.ViewModels.Management;
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

        #endregion
        
        #region Ctor

        public ManagementViewModel(
            IAudioService audioService,
            IDictionaryService dictionaryService)
        {
            //Instantiate child VMs
            DictionaryViewModel = new DictionaryViewModel(dictionaryService);
            OtherViewModel = new OtherViewModel();
            PointingAndSelectingViewModel = new PointingAndSelectingViewModel();
            SoundsViewModel = new SoundsViewModel(audioService);
            VisualsViewModel = new VisualsViewModel();
            WordsViewModel = new WordsViewModel(dictionaryService);
            
            //Instantiate interaction requests and commands
            ConfirmationRequest = new InteractionRequest<Confirmation>();
            OkCommand = new DelegateCommand<Window>(Ok); //Can always click Ok
            CancelCommand = new DelegateCommand<Window>(Cancel); //Can always click Cancel
        }
        
        #endregion
        
        #region Properties
        
        public bool ChangesRequireRestart
        {
            get
            {
                return DictionaryViewModel.ChangesRequireRestart
                    || OtherViewModel.ChangesRequireRestart
                    || PointingAndSelectingViewModel.ChangesRequireRestart
                    || SoundsViewModel.ChangesRequireRestart
                    || VisualsViewModel.ChangesRequireRestart
                    || WordsViewModel.ChangesRequireRestart;
            }
        }

        public DictionaryViewModel DictionaryViewModel { get; private set; }
        public OtherViewModel OtherViewModel { get; private set; }
        public PointingAndSelectingViewModel PointingAndSelectingViewModel { get; private set; }
        public SoundsViewModel SoundsViewModel { get; private set; }
        public VisualsViewModel VisualsViewModel { get; private set; }
        public WordsViewModel WordsViewModel { get; private set; }
        
        public InteractionRequest<Confirmation> ConfirmationRequest { get; private set; }
        public DelegateCommand<Window> OkCommand { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        
        #endregion
        
        #region Methods

        private void ApplyChanges()
        {
            DictionaryViewModel.ApplyChanges();
            OtherViewModel.ApplyChanges();
            PointingAndSelectingViewModel.ApplyChanges();
            SoundsViewModel.ApplyChanges();
            VisualsViewModel.ApplyChanges();
            WordsViewModel.ApplyChanges();
        }

        private void Ok(Window window)
        {
            if (ChangesRequireRestart)
            {
                //Warn if restart required and prompt for Confirmation before restarting
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = "May I restart ETTA?",
                        Content = "ETTA needs to restart to apply your changes.\nPlease click OK to continue with the restart, or CANCEL to discard your changes"
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            ApplyChanges();
                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                    });
            }
            else
            {
                ApplyChanges();
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
