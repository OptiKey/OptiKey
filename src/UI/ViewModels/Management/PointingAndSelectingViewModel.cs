using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

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

                    case Enums.TriggerSources.KeyboardKeyDownsUps:
                    case Enums.TriggerSources.MouseButtonDownUps:
                        MultiKeySelectionTriggerStopSignal = Enums.TriggerStopSignals.NextLow;
                    break;
                }
            });

            this.OnPropertyChanges(vm => vm.ProgressIndicatorBehaviour).Subscribe(pib =>
            {
                if (pib == Enums.ProgressIndicatorBehaviours.Grow &&
                    ProgressIndicatorResizeStartProportion > ProgressIndicatorResizeEndProportion)
                {
                    var endProportion = ProgressIndicatorResizeEndProportion;
                    ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeStartProportion;
                    ProgressIndicatorResizeStartProportion = endProportion;
                }
                else if (pib == Enums.ProgressIndicatorBehaviours.Shrink &&
                    ProgressIndicatorResizeStartProportion < ProgressIndicatorResizeEndProportion)
                {
                    var endProportion = ProgressIndicatorResizeEndProportion;
                    ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeStartProportion;
                    ProgressIndicatorResizeStartProportion = endProportion;
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
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.GazeTracker.ToDescription(), Enums.PointsSources.GazeTracker),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TheEyeTribe.ToDescription(), Enums.PointsSources.TheEyeTribe),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiEyeX.ToDescription(), Enums.PointsSources.TobiiEyeX),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiRex.ToDescription(), Enums.PointsSources.TobiiRex),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiPcEyeGo.ToDescription(), Enums.PointsSources.TobiiPcEyeGo),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.MousePosition.ToDescription(), Enums.PointsSources.MousePosition)
                };
            }
        }
        
        public List<KeyValuePair<string, TriggerSources>> TriggerSources
        {
            get
            {
                return new List<KeyValuePair<string, TriggerSources>>
                {
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.Fixations.ToDescription(), Enums.TriggerSources.Fixations),
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.KeyboardKeyDownsUps.ToDescription(), Enums.TriggerSources.KeyboardKeyDownsUps),
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.MouseButtonDownUps.ToDescription(), Enums.TriggerSources.MouseButtonDownUps)
                };
            }
        }
        
        public List<Keys> Keys
        {
            get { return Enum.GetValues(typeof(Enums.Keys)).Cast<Enums.Keys>().OrderBy(k => k.ToString()).ToList(); }
        }
        
        public List<MouseButtons> MouseButtons
        {
            get { return Enum.GetValues(typeof(Enums.MouseButtons)).Cast<Enums.MouseButtons>().OrderBy(mb => mb.ToString()).ToList(); }
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

        public List<KeyValuePair<string, ProgressIndicatorBehaviours>> ProgressIndicatorBehaviours
        {
            get
            {
                return new List<KeyValuePair<string, ProgressIndicatorBehaviours>>
                {
                    new KeyValuePair<string, ProgressIndicatorBehaviours>("Fill Pie", Enums.ProgressIndicatorBehaviours.FillPie),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>("Grow", Enums.ProgressIndicatorBehaviours.Grow),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>("Shrink", Enums.ProgressIndicatorBehaviours.Shrink)
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

        private Keys keySelectionTriggerKeyboardKeyDownUpKey;
        public Keys KeySelectionTriggerKeyboardKeyDownUpKey
        {
            get { return keySelectionTriggerKeyboardKeyDownUpKey; }
            set { SetProperty(ref keySelectionTriggerKeyboardKeyDownUpKey, value); }
        }

        private MouseButtons keySelectionTriggerMouseDownUpButton;
        public MouseButtons KeySelectionTriggerMouseDownUpButton
        {
            get { return keySelectionTriggerMouseDownUpButton; }
            set { SetProperty(ref keySelectionTriggerMouseDownUpButton, value); }
        }
        
        private double keySelectionTriggerFixationLockOnTimeInMs;
        public double KeySelectionTriggerFixationLockOnTimeInMs
        {
            get { return keySelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationLockOnTimeInMs, value); }
        }

        private bool keySelectionTriggerFixationResumeRequiresLockOn;
        public bool KeySelectionTriggerFixationResumeRequiresLockOn
        {
            get { return keySelectionTriggerFixationResumeRequiresLockOn; }
            set { SetProperty(ref keySelectionTriggerFixationResumeRequiresLockOn, value); }
        }

        private double keySelectionTriggerFixationCompleteTimeInMs;
        public double KeySelectionTriggerFixationCompleteTimeInMs
        {
            get { return keySelectionTriggerFixationCompleteTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTimeInMs, value); }
        }

        private double keySelectionTriggerIncompleteFixationTtlInMs;
        public double KeySelectionTriggerIncompleteFixationTtlInMs
        {
            get { return keySelectionTriggerIncompleteFixationTtlInMs; }
            set { SetProperty(ref keySelectionTriggerIncompleteFixationTtlInMs, value); }
        }

        private TriggerSources pointSelectionTriggerSource;
        public TriggerSources PointSelectionTriggerSource
        {
            get { return pointSelectionTriggerSource; }
            set { SetProperty(ref pointSelectionTriggerSource, value); }
        }

        private Keys pointSelectionTriggerKeyboardKeyDownUpKey;
        public Keys PointSelectionTriggerKeyboardKeyDownUpKey
        {
            get { return pointSelectionTriggerKeyboardKeyDownUpKey; }
            set { SetProperty(ref pointSelectionTriggerKeyboardKeyDownUpKey, value); }
        }

        private MouseButtons pointSelectionTriggerMouseDownUpButton;
        public MouseButtons PointSelectionTriggerMouseDownUpButton
        {
            get { return pointSelectionTriggerMouseDownUpButton; }
            set { SetProperty(ref pointSelectionTriggerMouseDownUpButton, value); }
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

        private double pointSelectionTriggerLockOnRadius;
        public double PointSelectionTriggerLockOnRadiusInPixels
        {
            get { return pointSelectionTriggerLockOnRadius; }
            set { SetProperty(ref pointSelectionTriggerLockOnRadius, value); }
        }

        private double pointSelectionTriggerFixationRadius;
        public double PointSelectionTriggerFixationRadiusInPixels
        {
            get { return pointSelectionTriggerFixationRadius; }
            set { SetProperty(ref pointSelectionTriggerFixationRadius, value); }
        }

        private ProgressIndicatorBehaviours progressIndicatorBehaviour;
        public ProgressIndicatorBehaviours ProgressIndicatorBehaviour
        {
            get { return progressIndicatorBehaviour; }
            set { SetProperty(ref progressIndicatorBehaviour, value); }
        }

        private int progressIndicatorResizeStartProportion;
        public int ProgressIndicatorResizeStartProportion
        {
            get { return progressIndicatorResizeStartProportion; }
            set { SetProperty(ref progressIndicatorResizeStartProportion, value); }
        }

        private int progressIndicatorResizeEndProportion;
        public int ProgressIndicatorResizeEndProportion
        {
            get { return progressIndicatorResizeEndProportion; }
            set { SetProperty(ref progressIndicatorResizeEndProportion, value); }
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
                    || (Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey != KeySelectionTriggerKeyboardKeyDownUpKey && KeySelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps)
                    || (Settings.Default.KeySelectionTriggerMouseDownUpButton != KeySelectionTriggerMouseDownUpButton && KeySelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps)
                    || (Settings.Default.KeySelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn != KeySelectionTriggerFixationResumeRequiresLockOn && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerIncompleteFixationTtl != TimeSpan.FromMilliseconds(KeySelectionTriggerIncompleteFixationTtlInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || Settings.Default.PointSelectionTriggerSource != PointSelectionTriggerSource
                    || (Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey != PointSelectionTriggerKeyboardKeyDownUpKey && PointSelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps)
                    || (Settings.Default.PointSelectionTriggerMouseDownUpButton != PointSelectionTriggerMouseDownUpButton && PointSelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps)
                    || (Settings.Default.PointSelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationLockOnTimeInMs) && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.PointSelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(PointSelectionTriggerFixationCompleteTimeInMs) && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.PointSelectionTriggerLockOnRadiusInPixels != PointSelectionTriggerLockOnRadiusInPixels && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.PointSelectionTriggerFixationRadiusInPixels != PointSelectionTriggerFixationRadiusInPixels && PointSelectionTriggerSource == Enums.TriggerSources.Fixations)
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
            KeySelectionTriggerKeyboardKeyDownUpKey = Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey;
            KeySelectionTriggerMouseDownUpButton = Settings.Default.KeySelectionTriggerMouseDownUpButton;
            KeySelectionTriggerFixationLockOnTimeInMs = Settings.Default.KeySelectionTriggerFixationLockOnTime.TotalMilliseconds;
            KeySelectionTriggerFixationResumeRequiresLockOn = Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn;
            KeySelectionTriggerFixationCompleteTimeInMs = Settings.Default.KeySelectionTriggerFixationCompleteTime.TotalMilliseconds;
            KeySelectionTriggerIncompleteFixationTtlInMs = Settings.Default.KeySelectionTriggerIncompleteFixationTtl.TotalMilliseconds;
            PointSelectionTriggerSource = Settings.Default.PointSelectionTriggerSource;
            PointSelectionTriggerKeyboardKeyDownUpKey = Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey;
            PointSelectionTriggerMouseDownUpButton = Settings.Default.PointSelectionTriggerMouseDownUpButton;
            PointSelectionTriggerFixationLockOnTimeInMs = Settings.Default.PointSelectionTriggerFixationLockOnTime.TotalMilliseconds;
            PointSelectionTriggerFixationCompleteTimeInMs = Settings.Default.PointSelectionTriggerFixationCompleteTime.TotalMilliseconds;
            PointSelectionTriggerLockOnRadiusInPixels = Settings.Default.PointSelectionTriggerLockOnRadiusInPixels;
            PointSelectionTriggerFixationRadiusInPixels = Settings.Default.PointSelectionTriggerFixationRadiusInPixels;
            ProgressIndicatorBehaviour = Settings.Default.ProgressIndicatorBehaviour;
            ProgressIndicatorResizeStartProportion = Settings.Default.ProgressIndicatorResizeStartProportion;
            ProgressIndicatorResizeEndProportion = Settings.Default.ProgressIndicatorResizeEndProportion;
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
            Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey = KeySelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.KeySelectionTriggerMouseDownUpButton = KeySelectionTriggerMouseDownUpButton;
            Settings.Default.KeySelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn = KeySelectionTriggerFixationResumeRequiresLockOn;
            Settings.Default.KeySelectionTriggerFixationCompleteTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs);
            Settings.Default.KeySelectionTriggerIncompleteFixationTtl = TimeSpan.FromMilliseconds(KeySelectionTriggerIncompleteFixationTtlInMs);
            Settings.Default.PointSelectionTriggerSource = PointSelectionTriggerSource;
            Settings.Default.PointSelectionTriggerKeyboardKeyDownUpKey = PointSelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.PointSelectionTriggerMouseDownUpButton = PointSelectionTriggerMouseDownUpButton;
            Settings.Default.PointSelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(PointSelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.PointSelectionTriggerFixationCompleteTime = TimeSpan.FromMilliseconds(PointSelectionTriggerFixationCompleteTimeInMs);
            Settings.Default.PointSelectionTriggerLockOnRadiusInPixels = PointSelectionTriggerLockOnRadiusInPixels;
            Settings.Default.PointSelectionTriggerFixationRadiusInPixels = PointSelectionTriggerFixationRadiusInPixels;
            Settings.Default.ProgressIndicatorBehaviour = ProgressIndicatorBehaviour;
            Settings.Default.ProgressIndicatorResizeStartProportion = ProgressIndicatorResizeStartProportion;
            Settings.Default.ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeEndProportion;
            Settings.Default.MultiKeySelectionTriggerStopSignal = MultiKeySelectionTriggerStopSignal;
            Settings.Default.MultiKeySelectionFixationMinDwellTime = TimeSpan.FromMilliseconds(MultiKeySelectionFixationMinDwellTimeInMs);
            Settings.Default.MultiKeySelectionMaxDuration = TimeSpan.FromMilliseconds(MultiKeySelectionMaxDurationInMs);
        }

        #endregion
    }
}
