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

        public List<KeyValuePair<string, PointsSources>> PointsSources
        {
            get
            {
                return new List<KeyValuePair<string, PointsSources>>
                {
                    new KeyValuePair<string, string>("Gaze Tracker", PointsSources.GazeTracker),
                    new KeyValuePair<string, string>("The Eye Tribe", PointsSources.TheEyeTribe),
                    new KeyValuePair<string, string>("Mouse Position", PointsSources.MousePosition)
                };
            }
        }
        
        public List<KeyValuePair<string, TriggerSources>> TriggerSources
        {
            get
            {
                return new List<KeyValuePair<string, TriggerSources>>
                {
                    new KeyValuePair<string, string>("Fixations", TriggerSources.Fixations),
                    new KeyValuePair<string, string>("Keyboard Key", TriggerSources.KeyboardKeyDownsUps),
                    new KeyValuePair<string, string>("Mouse Button", TriggerSources.MouseButtonDownUps)
                };
            }
        }
        
        public List<Keys> Keys
        {
            get { return Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList(); }
        }
        
        public List<MouseButtons> MouseButtons
        {
            get { return Enum.GetValues(typeof(MouseButtons)).Cast<MouseButtons>().ToList(); }
        }
        
        public List<KeyValuePair<string, TriggerStopSignals>> TriggerStopSignals
        {
            get
            {
                return new List<KeyValuePair<string, TriggerStopSignals>>
                {
                    new KeyValuePair<string, string>("Trigger button/key pressed again", TriggerStopSignals.NextHigh),
                    new KeyValuePair<string, string>("Trigger button/key released", TriggerStopSignals.NextLow)
                };
            }
        }
        
        private PointsSources pointSource;
        public PointsSources PointsSource
        {
            get { return pointSource; }
            set { SetProperty(ref pointSource, value); }
        }
        
        private TimeSpan pointsMousePositionSampleInterval;
        public TimeSpan PointsMousePositionSampleInterval
        {
            get { return pointsMousePositionSampleInterval; }
            set { SetProperty(ref pointsMousePositionSampleInterval, value); }
        }
        
        private TimeSpan pointTtl;
        public TimeSpan PointTtl
        {
            get { return pointTtl; }
            set { SetProperty(ref pointTtl, value); }
        }
        
        private TriggerSources keySelectionTriggerSource;
        public TriggerSources KeySelectionTriggerSource
        {
            get { return keySelectionTriggerSource; }
            set { SetProperty(ref keySelectionTriggerSource, value); }
        }
        
        private TriggerSources pointSelectionTriggerSource;
        public TriggerSources PointSelectionTriggerSource
        {
            get { return pointSelectionTriggerSource; }
            set { SetProperty(ref pointSelectionTriggerSource, value); }
        }
        
        private Keys selectionTriggerKeyboardKeyDownUpKey;
        public Keys SelectionTriggerKeyboardKeyDownUpKey
        {
            get { return selectionTriggerKeyboardKeyDownUpKey; }
            set { SetProperty(ref selectionTriggerKeyboardKeyDownUpKey, value); }
        }
        
        private MouseButtons selectionTriggerMouseDownUpButton;
        public MouseButtons SelectionTriggerMouseDownUpButton
        {
            get { return selectionTriggerMouseDownUpButton; }
            set { SetProperty(ref selectionTriggerMouseDownUpButton, value); }
        }
        
        private TimeSpan pointSelectionTriggerFixationLockOnTime;
        public TimeSpan PointSelectionTriggerFixationLockOnTime
        {
            get { return pointSelectionTriggerFixationLockOnTime; }
            set { SetProperty(ref pointSelectionTriggerFixationLockOnTime, value); }
        }
        
        private TimeSpan pointSelectionTriggerFixationCompleteTime;
        public TimeSpan PointSelectionTriggerFixationCompleteTime
        {
            get { return pointSelectionTriggerFixationCompleteTime; }
            set { SetProperty(ref pointSelectionTriggerFixationCompleteTime, value); }
        }
        
        private double pointSelectionTriggerFixationRadius;
        public double PointSelectionTriggerFixationRadius
        {
            get { return pointSelectionTriggerFixationRadius; }
            set { SetProperty(ref pointSelectionTriggerFixationRadius, value); }
        }
        
        private TimeSpan keySelectionTriggerFixationLockOnTime;
        public TimeSpan KeySelectionTriggerFixationLockOnTime
        {
            get { return keySelectionTriggerFixationLockOnTime; }
            set { SetProperty(ref keySelectionTriggerFixationLockOnTime, value); }
        }
        
        private TimeSpan keySelectionTriggerFixationCompleteTime;
        public TimeSpan KeySelectionTriggerFixationCompleteTime
        {
            get { return keySelectionTriggerFixationCompleteTime; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTime, value); }
        }
        
        private TriggerStopSignals selectionTriggerStopSignal;
        public TriggerStopSignals SelectionTriggerStopSignal
        {
            get { return selectionTriggerStopSignal; }
            set { SetProperty(ref selectionTriggerStopSignal, value); }
        }
        
        private TimeSpan multiKeySelectionFixationMinDwellTime;
        public TimeSpan MultiKeySelectionFixationMinDwellTime
        {
            get { return multiKeySelectionFixationMinDwellTime; }
            set { SetProperty(ref multiKeySelectionFixationMinDwellTime, value); }
        }
        
        private TimeSpan multiKeySelectionMaxDuration;
        public TimeSpan MultiKeySelectionMaxDuration
        {
            get { return multiKeySelectionMaxDuration; }
            set { SetProperty(ref multiKeySelectionMaxDuration, value); }
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
            PointsMousePositionSampleInterval = Settings.Default.PointsMousePositionSampleInterval;
            PointTtl = Settings.Default.PointTtl;
            KeySelectionTriggerSource = Settings.Default.KeySelectionTriggerSource;
            PointSelectionTriggerSource = Settings.Default.PointSelectionTriggerSource;
            SelectionTriggerKeyboardKeyDownUpKey = Settings.Default.SelectionTriggerKeyboardKeyDownUpKey;
            SelectionTriggerMouseDownUpButton = Settings.Default.SelectionTriggerMouseDownUpButton;
            PointSelectionTriggerFixationLockOnTime = Settings.Default.PointSelectionTriggerFixationLockOnTime;
            PointSelectionTriggerFixationCompleteTime = Settings.Default.PointSelectionTriggerFixationCompleteTime;
            PointSelectionTriggerFixationRadius = Settings.Default.PointSelectionTriggerFixationRadius;
            KeySelectionTriggerFixationLockOnTime = Settings.Default.KeySelectionTriggerFixationLockOnTime;
            KeySelectionTriggerFixationCompleteTime = Settings.Default.KeySelectionTriggerFixationCompleteTime;
            SelectionTriggerStopSignal = Settings.Default.SelectionTriggerStopSignal;
            MultiKeySelectionFixationMinDwellTime = Settings.Default.MultiKeySelectionFixationMinDwellTime;
            MultiKeySelectionMaxDuration = Settings.Default.MultiKeySelectionMaxDuration;
        }

        public void ApplyChanges()
        {
            Settings.Default.PointsSource = PointsSource;
            Settings.Default.PointsMousePositionSampleInterval = PointsMousePositionSampleInterval;
            Settings.Default.PointTtl = PointTtl;
            Settings.Default.KeySelectionTriggerSource = KeySelectionTriggerSource;
            Settings.Default.PointSelectionTriggerSource = PointSelectionTriggerSource;
            Settings.Default.SelectionTriggerKeyboardKeyDownUpKey = SelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.SelectionTriggerMouseDownUpButton = SelectionTriggerMouseDownUpButton;
            Settings.Default.PointSelectionTriggerFixationLockOnTime = PointSelectionTriggerFixationLockOnTime;
            Settings.Default.PointSelectionTriggerFixationCompleteTime = PointSelectionTriggerFixationCompleteTime;
            Settings.Default.PointSelectionTriggerFixationRadius = PointSelectionTriggerFixationRadius;
            Settings.Default.KeySelectionTriggerFixationLockOnTime = KeySelectionTriggerFixationLockOnTime;
            Settings.Default.KeySelectionTriggerFixationCompleteTime = KeySelectionTriggerFixationCompleteTime;
            Settings.Default.SelectionTriggerStopSignal = SelectionTriggerStopSignal;
            Settings.Default.MultiKeySelectionFixationMinDwellTime = MultiKeySelectionFixationMinDwellTime;
            Settings.Default.MultiKeySelectionMaxDuration = MultiKeySelectionMaxDuration;
            Settings.Default.Save();
        }

        #endregion
    }
}
