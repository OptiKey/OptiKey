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
            //Debug = Settings.Default.Debug;
        }

        public void ApplyChanges()
        {
            //Settings.Default.Debug = Debug;
            
            //Settings.Default.Save();
        }

        #endregion
    }
}
