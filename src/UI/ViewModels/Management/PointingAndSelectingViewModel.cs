using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class PointingAndSelectingViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public PointingAndSelectingViewModel()
        {
            LoadSettings();
        }
        
        #endregion
        
        #region Properties

        public List<KeyValuePair<string, string>> PointsSources
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Gaze Tracker", PointsSources.GazeTracker),
                    new KeyValuePair<string, string>("The Eye Tribe", PointsSources.TheEyeTribe),
                    new KeyValuePair<string, string>("Mouse Position", PointsSources.MousePosition)
                };
            }
        }
        
        private PointsSources pointSource;
        public PointsSources PointsSource
        {
            get { return pointSource; }
            set { SetProperty(ref pointSource, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                Settings.Default.PointsSource != PointsSource
                  || (Settings.Default.PointsMousePositionSampleInterval != PointsMousePositionSampleInterval 
                     && PointsSource == PointSources.MousePosition)
                  || Settings.Default.PointTtl != PointTtl
                  || Settings.Default.KeySelectionTriggerSource != KeySelectionTriggerSource
                  || Settings.Default.PointSelectionTriggerSource != PointSelectionTriggerSource
                  || (Settings.Default.SelectionTriggerKeyboardKeyDownUpKey != SelectionTriggerKeyboardKeyDownUpKey 
                     && (KeySelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps || PointSelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps))
                  || (Settings.Default.SelectionTriggerMouseDownUpButton != SelectionTriggerMouseDownUpButton
                     && (KeySelectionTriggerSource == TriggerSources.MouseButtonDownUps || PointSelectionTriggerSource == TriggerSources.MouseButtonDownUps))
                  || (Settings.Default.PointSelectionTriggerFixationLockOnTime != PointSelectionTriggerFixationLockOnTime 
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.PointSelectionTriggerFixationCompleteTime != PointSelectionTriggerFixationCompleteTime 
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.PointSelectionTriggerFixationRadius != PointSelectionTriggerFixationRadius 
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.KeySelectionTriggerFixationLockOnTime != KeySelectionTriggerFixationLockOnTime 
                     && KeySelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.KeySelectionTriggerFixationCompleteTime != KeySelectionTriggerFixationCompleteTime 
                     && KeySelectionTriggerSource == TriggerSources.Fixations)
                  || Settings.Default.SelectionTriggerStopSignal != SelectionTriggerStopSignal
                  || Settings.Default.MultiKeySelectionFixationMinDwellTime != MultiKeySelectionFixationMinDwellTime
                  || Settings.Default.MultiKeySelectionMaxDuration != MultiKeySelectionMaxDuration;
            }
        }
        
        #endregion
        
        #region Methods

        private void LoadSettings()
        {
            PointsSource = Settings.Default.PointsSource;
        }

        public void ApplyChanges()
        {
            Settings.Default.PointsSource = PointsSource;
            Settings.Default.Save();
        }

        #endregion
    }
}
