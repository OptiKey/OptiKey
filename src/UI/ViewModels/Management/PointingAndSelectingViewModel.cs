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
            
            //Set up property defaulting logic
            this.PropertyChanges(vm => vm.KeySelectionTriggerSource).Subscribe(ts => 
                {
                    switch(ts) 
                    {
                        case TriggerSources.Fixations:
                            MultiKeySelectionTriggerStopSignal = TriggerStopSignals.NextHigh;
                        break;
                    }
                });
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
        
        private int pointsMousePositionSampleIntervalInMs;
        public int PointsMousePositionSampleIntervalInMs
        {
            get { return pointsMousePositionSampleIntervalInMs; }
            set { SetProperty(ref pointsMousePositionSampleIntervalInMs, value); }
        }
        
        private int pointTtlInMs;
        public int PointTtlInMs
        {
            get { return pointTtlInMs; }
            set { SetProperty(ref pointTtlInMs, value); }
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
        
        private int pointSelectionTriggerFixationLockOnTimeInMs;
        public int PointSelectionTriggerFixationLockOnTimeInMs
        {
            get { return pointSelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref pointSelectionTriggerFixationLockOnTimeInMs, value); }
        }
        
        private int pointSelectionTriggerFixationCompleteTimeInMs;
        public int PointSelectionTriggerFixationCompleteTimeInMs
        {
            get { return pointSelectionTriggerFixationCompleteTimeInMs; }
            set { SetProperty(ref pointSelectionTriggerFixationCompleteTimeInMs, value); }
        }
        
        private double pointSelectionTriggerFixationRadius;
        public double PointSelectionTriggerFixationRadius
        {
            get { return pointSelectionTriggerFixationRadius; }
            set { SetProperty(ref pointSelectionTriggerFixationRadius, value); }
        }
        
        private int keySelectionTriggerFixationLockOnTimeInMs;
        public int KeySelectionTriggerFixationLockOnTimeInMs
        {
            get { return keySelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationLockOnTimeInMs, value); }
        }
        
        private int keySelectionTriggerFixationCompleteTimeInMs;
        public int KeySelectionTriggerFixationCompleteTimeInMs
        {
            get { return keySelectionTriggerFixationCompleteTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTimeInMs, value); }
        }
        
        private TriggerStopSignals multiKeySelectionTriggerStopSignal;
        public TriggerStopSignals MultiKeySelectionTriggerStopSignal
        {
            get { return multiKeySelectionTriggerStopSignal; }
            set { SetProperty(ref multiKeySelectionTriggerStopSignal, value); }
        }
        
        private int multiKeySelectionFixationMinDwellTimeInMs;
        public int MultiKeySelectionFixationMinDwellTimeInMs
        {
            get { return multiKeySelectionFixationMinDwellTimeInMs; }
            set { SetProperty(ref multiKeySelectionFixationMinDwellTimeInMs, value); }
        }
        
        private int multiKeySelectionMaxDurationInMs;
        public int MultiKeySelectionMaxDurationInMs
        {
            get { return multiKeySelectionMaxDurationInMs; }
            set { SetProperty(ref multiKeySelectionMaxDurationInMs, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                Settings.Default.PointsSource != PointsSource
                  || (Settings.Default.PointsMousePositionSampleInterval != TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs)
                     && PointsSource == PointSources.MousePosition)
                  || Settings.Default.PointTtl != TimeSpan.FromMilliseconds(PointTtlInMs)
                  || Settings.Default.KeySelectionTriggerSource != KeySelectionTriggerSource
                  || Settings.Default.PointSelectionTriggerSource != PointSelectionTriggerSource
                  || (Settings.Default.SelectionTriggerKeyboardKeyDownUpKey != SelectionTriggerKeyboardKeyDownUpKey 
                     && (KeySelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps || PointSelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps))
                  || (Settings.Default.SelectionTriggerMouseDownUpButton != SelectionTriggerMouseDownUpButton
                     && (KeySelectionTriggerSource == TriggerSources.MouseButtonDownUps || PointSelectionTriggerSource == TriggerSources.MouseButtonDownUps))
                  || (Settings.Default.PointSelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationLockOnTimeInMs)
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.PointSelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationCompleteTimeInMs)
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.PointSelectionTriggerFixationRadius != PointSelectionTriggerFixationRadius 
                     && PointSelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.KeySelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs)
                     && KeySelectionTriggerSource == TriggerSources.Fixations)
                  || (Settings.Default.KeySelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs)
                     && KeySelectionTriggerSource == TriggerSources.Fixations)
                  || Settings.Default.MultiKeySelectionTriggerStopSignal != MultiKeySelectionTriggerStopSignal
                  || Settings.Default.MultiKeySelectionFixationMinDwellTime != TimeSpan.FromMilliseconds(MultiKeySelectionFixationMinDwellTimeInMs)
                  || Settings.Default.MultiKeySelectionMaxDuration != TimeSpan.FromMilliseconds(MultiKeySelectionMaxDurationInMs);
            }
        }
        
        #endregion
        
        #region Methods

        private void LoadSettings()
        {
            PointsSource = Settings.Default.PointsSource;
            PointsMousePositionSampleIntervalInMs = Settings.Default.PointsMousePositionSampleInterval.TotalMilliseconds;
            PointTtlInMs = Settings.Default.PointTtl.TotalMilliseconds;
            KeySelectionTriggerSource = Settings.Default.KeySelectionTriggerSource;
            PointSelectionTriggerSource = Settings.Default.PointSelectionTriggerSource;
            SelectionTriggerKeyboardKeyDownUpKey = Settings.Default.SelectionTriggerKeyboardKeyDownUpKey;
            SelectionTriggerMouseDownUpButton = Settings.Default.SelectionTriggerMouseDownUpButton;
            PointSelectionTriggerFixationLockOnTimeInMs = Settings.Default.PointSelectionTriggerFixationLockOnTime.TotalMilliseconds;
            PointSelectionTriggerFixationCompleteTimeInMs = Settings.Default.PointSelectionTriggerFixationCompleteTime.TotalMilliseconds;
            PointSelectionTriggerFixationRadius = Settings.Default.PointSelectionTriggerFixationRadius;
            KeySelectionTriggerFixationLockOnTimeInMs = Settings.Default.KeySelectionTriggerFixationLockOnTime.TotalMilliseconds;
            KeySelectionTriggerFixationCompleteTimeInMs = Settings.Default.KeySelectionTriggerFixationCompleteTime.TotalMilliseconds;
            MultiKeySelectionTriggerStopSignal = Settings.Default.MultiKeySelectionTriggerStopSignal;
            MultiKeySelectionFixationMinDwellTimeInMs = Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds;
            MultiKeySelectionMaxDurationInMs = Settings.Default.MultiKeySelectionMaxDuration.TotalMilliseconds;
        }

        public void ApplyChanges()
        {
            Settings.Default.PointsSource = PointsSource;
            Settings.Default.PointsMousePositionSampleInterval = TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs);
            Settings.Default.PointTtl = TimeSpan.FromMilliseconds(PointTtlInMs);
            Settings.Default.KeySelectionTriggerSource = KeySelectionTriggerSource;
            Settings.Default.PointSelectionTriggerSource = PointSelectionTriggerSource;
            Settings.Default.SelectionTriggerKeyboardKeyDownUpKey = SelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.SelectionTriggerMouseDownUpButton = SelectionTriggerMouseDownUpButton;
            Settings.Default.PointSelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(PointSelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.PointSelectionTriggerFixationCompleteTime = TimeSpan.FromMilliseconds(PointSelectionTriggerFixationCompleteTimeInMs);
            Settings.Default.PointSelectionTriggerFixationRadius = PointSelectionTriggerFixationRadius;
            Settings.Default.KeySelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.KeySelectionTriggerFixationCompleteTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs);
            Settings.Default.SelectionTriggerStopSignal = SelectionTriggerStopSignal;
            Settings.Default.MultiKeySelectionFixationMinDwellTime = TimeSpan.FromMilliseconds(MultiKeySelectionFixationMinDwellTimeInMs);
            Settings.Default.MultiKeySelectionMaxDuration = TimeSpan.FromMilliseconds(MultiKeySelectionMaxDurationInMs);
            Settings.Default.Save();
        }

        #endregion
    }
}
