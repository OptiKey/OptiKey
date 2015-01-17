using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class PointingAndSelectingViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public PointingAndSelectingViewModel()
        {
            Load();
            
            //Set up property defaulting logic
            this.OnPropertyChanges(vm => vm.KeySelectionTriggerSource).Subscribe(ts => 
                {
                    switch(ts) 
                    {
                        case Enums.TriggerSources.Fixations:
                            MultiKeySelectionTriggerStopSignal = Enums.TriggerStopSignals.NextHigh;
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
                    new KeyValuePair<string, PointsSources>("Gaze Tracker", Enums.PointsSources.GazeTracker),
                    new KeyValuePair<string, PointsSources>("The Eye Tribe", Enums.PointsSources.TheEyeTribe),
                    new KeyValuePair<string, PointsSources>("Mouse Position", Enums.PointsSources.MousePosition)
                };
            }
        }
        
        public List<KeyValuePair<string, TriggerSources>> TriggerSources
        {
            get
            {
                return new List<KeyValuePair<string, TriggerSources>>
                {
                    new KeyValuePair<string, TriggerSources>("Fixations", Enums.TriggerSources.Fixations),
                    new KeyValuePair<string, TriggerSources>("Keyboard Key", Enums.TriggerSources.KeyboardKeyDownsUps),
                    new KeyValuePair<string, TriggerSources>("Mouse Button", Enums.TriggerSources.MouseButtonDownUps)
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
                    new KeyValuePair<string, TriggerStopSignals>("Trigger button/key pressed again", Enums.TriggerStopSignals.NextHigh),
                    new KeyValuePair<string, TriggerStopSignals>("Trigger button/key released", Enums.TriggerStopSignals.NextLow)
                };
            }
        }
        
        private PointsSources pointSource;
        public PointsSources PointsSource
        {
            get { return pointSource; }
            set { SetProperty(ref pointSource, value); }
        }

        private double pointsMousePositionSampleIntervalInMs;
        public double PointsMousePositionSampleIntervalInMs
        {
            get { return pointsMousePositionSampleIntervalInMs; }
            set { SetProperty(ref pointsMousePositionSampleIntervalInMs, value); }
        }

        private double pointTtlInMs;
        public double PointTtlInMs
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

        private double pointSelectionTriggerFixationLockOnTimeInMs;
        public double PointSelectionTriggerFixationLockOnTimeInMs
        {
            get { return pointSelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref pointSelectionTriggerFixationLockOnTimeInMs, value); }
        }

        private double pointSelectionTriggerFixationCompleteTimeInMs;
        public double PointSelectionTriggerFixationCompleteTimeInMs
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

        private double keySelectionTriggerFixationLockOnTimeInMs;
        public double KeySelectionTriggerFixationLockOnTimeInMs
        {
            get { return keySelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationLockOnTimeInMs, value); }
        }

        private double keySelectionTriggerFixationCompleteTimeInMs;
        public double KeySelectionTriggerFixationCompleteTimeInMs
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

        private double multiKeySelectionFixationMinDwellTimeInMs;
        public double MultiKeySelectionFixationMinDwellTimeInMs
        {
            get { return multiKeySelectionFixationMinDwellTimeInMs; }
            set { SetProperty(ref multiKeySelectionFixationMinDwellTimeInMs, value); }
        }
        
        private double multiKeySelectionMaxDurationInMs;
        public double MultiKeySelectionMaxDurationInMs
        {
            get { return multiKeySelectionMaxDurationInMs; }
            set { SetProperty(ref multiKeySelectionMaxDurationInMs, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                return Settings.Default.PointsSource != PointsSource
                    || (Settings.Default.PointsMousePositionSampleInterval != TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs) && PointsSource == Enums.PointsSources.MousePosition)
                    || Settings.Default.PointTtl != TimeSpan.FromMilliseconds(PointTtlInMs)
                    || Settings.Default.KeySelectionTriggerSource != KeySelectionTriggerSource
                    || Settings.Default.PointSelectionTriggerSource != PointSelectionTriggerSource
                    || (Settings.Default.SelectionTriggerKeyboardKeyDownUpKey != SelectionTriggerKeyboardKeyDownUpKey && (KeySelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps || PointSelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps))
                    || (Settings.Default.SelectionTriggerMouseDownUpButton != SelectionTriggerMouseDownUpButton && (KeySelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps || PointSelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps))
                    || (Settings.Default.PointSelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationLockOnTimeInMs) && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.PointSelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationCompleteTimeInMs) && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.PointSelectionTriggerFixationRadius != PointSelectionTriggerFixationRadius && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || Settings.Default.MultiKeySelectionTriggerStopSignal != MultiKeySelectionTriggerStopSignal
                    || Settings.Default.MultiKeySelectionFixationMinDwellTime != TimeSpan.FromMilliseconds(MultiKeySelectionFixationMinDwellTimeInMs)
                    || Settings.Default.MultiKeySelectionMaxDuration != TimeSpan.FromMilliseconds(MultiKeySelectionMaxDurationInMs);
            }
        }
        
        #endregion
        
        #region Methods

        private void Load()
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
            Settings.Default.MultiKeySelectionTriggerStopSignal = MultiKeySelectionTriggerStopSignal;
            Settings.Default.MultiKeySelectionFixationMinDwellTime = TimeSpan.FromMilliseconds(MultiKeySelectionFixationMinDwellTimeInMs);
            Settings.Default.MultiKeySelectionMaxDuration = TimeSpan.FromMilliseconds(MultiKeySelectionMaxDurationInMs);
            Settings.Default.Save();
        }

        #endregion
    }
}
