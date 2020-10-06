// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class PointingAndSelectingViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public PointingAndSelectingViewModel()
        {
            Load();

            //Set up property defaulting logic
            this.OnPropertyChanges(vm => vm.KeySelectionTriggerSource).Subscribe(ts =>
            {
                switch (ts)
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

        public static List<KeyValuePair<string, PointsSources>> PointsSources
        {
            get
            {
                return new List<KeyValuePair<string, PointsSources>>
                {
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.Alienware17.ToDescription(), Enums.PointsSources.Alienware17),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.GazeTracker.ToDescription(), Enums.PointsSources.GazeTracker),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.IrisbondDuo.ToDescription(), Enums.PointsSources.IrisbondDuo),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.MousePosition.ToDescription(), Enums.PointsSources.MousePosition),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.SteelseriesSentry.ToDescription(), Enums.PointsSources.SteelseriesSentry),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TheEyeTribe.ToDescription(), Enums.PointsSources.TheEyeTribe),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiEyeTracker4C.ToDescription(), Enums.PointsSources.TobiiEyeTracker4C),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiEyeTracker5.ToDescription(), Enums.PointsSources.TobiiEyeTracker5),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiEyeX.ToDescription(), Enums.PointsSources.TobiiEyeX),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiPcEyeGo.ToDescription(), Enums.PointsSources.TobiiPcEyeGo),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiPcEyeGoPlus.ToDescription(), Enums.PointsSources.TobiiPcEyeGoPlus),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiPcEyeMini.ToDescription(), Enums.PointsSources.TobiiPcEyeMini),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiRex.ToDescription(), Enums.PointsSources.TobiiRex),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiX2_30.ToDescription(), Enums.PointsSources.TobiiX2_30),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiX2_60.ToDescription(), Enums.PointsSources.TobiiX2_60),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.VisualInteractionMyGaze.ToDescription(), Enums.PointsSources.VisualInteractionMyGaze)
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

        public List<KeyValuePair<string, DataStreamProcessingLevels>> DataStreamProcessingLevels
        {
            get
            {
                return new List<KeyValuePair<string, DataStreamProcessingLevels>>
                {
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.None.ToDescription(), Enums.DataStreamProcessingLevels.None),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.Low.ToDescription(), Enums.DataStreamProcessingLevels.Low),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.Medium.ToDescription(), Enums.DataStreamProcessingLevels.Medium),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.High.ToDescription(), Enums.DataStreamProcessingLevels.High)
                };
            }
        }

        public List<KeyValuePair<string, DataStreamProcessingLevels>> IrisbondDataStreamProcessingLevels
        {
            get
            {
                return new List<KeyValuePair<string, DataStreamProcessingLevels>>
                {
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.None.ToDescription(), Enums.DataStreamProcessingLevels.None),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.Medium.ToDescription(), Enums.DataStreamProcessingLevels.Medium)
                };
            }
        }

        public List<KeyValuePair<string, int>> GazeSmoothingLevels
        {
            get
            {
                return new List<KeyValuePair<string, int>>
                {
                    new KeyValuePair<string, int>
                        ("0 - " + Resources.NONE, 0),
                    new KeyValuePair<string, int>
                        ("1 - " + Resources.LOW,  1),
                    new KeyValuePair<string, int>
                        ("2 - " + Resources.MEDIUM, 2),
                    new KeyValuePair<string, int>
                        ("3 - " + Resources.HIGH, 3),
                    new KeyValuePair<string, int>
                        ("4 - Ultra", 4),
                    new KeyValuePair<string, int>
                        ("5 - Extreme", 5),
                    new KeyValuePair<string, int>
                        ("6 - XXtreme", 6),
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
                    new KeyValuePair<string, TriggerStopSignals>(Resources.TRIGGER_PRESSED_AGAIN, Enums.TriggerStopSignals.NextHigh),
                    new KeyValuePair<string, TriggerStopSignals>(Resources.TRIGGER_RELEASED, Enums.TriggerStopSignals.NextLow)
                };
            }
        }

        public List<KeyValuePair<string, ProgressIndicatorBehaviours>> ProgressIndicatorBehaviours
        {
            get
            {
                return new List<KeyValuePair<string, ProgressIndicatorBehaviours>>
                {
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.FILL_PIE, Enums.ProgressIndicatorBehaviours.FillPie),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.GROW, Enums.ProgressIndicatorBehaviours.Grow),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.SHRINK_INDICATOR, Enums.ProgressIndicatorBehaviours.Shrink)
                };
            }
        }

        private PointsSources pointSource;
        public PointsSources PointsSource
        {
            get { return pointSource; }
            set { SetProperty(ref pointSource, value); }
        }

        private DataStreamProcessingLevels tobiiEyeXProcessingLevel;
        public DataStreamProcessingLevels TobiiEyeXProcessingLevel
        {
            get { return tobiiEyeXProcessingLevel; }
            set { SetProperty(ref tobiiEyeXProcessingLevel, value); }
        }

        private DataStreamProcessingLevels irisBondProcessingLevel;
        public DataStreamProcessingLevels IrisbondProcessingLevel
        {
            get { return irisBondProcessingLevel; }
            set { SetProperty(ref irisBondProcessingLevel, value); }
        }

        private int gazeSmoothingLevel;
        public int GazeSmoothingLevel
        {
            get { return gazeSmoothingLevel; }
            set { SetProperty(ref gazeSmoothingLevel, value); }
        }

        private bool smoothWhenChangingGazeTarget;
        public bool SmoothWhenChangingGazeTarget
        {
            get { return smoothWhenChangingGazeTarget; }
            set { SetProperty(ref smoothWhenChangingGazeTarget, value); }
        }

        private double pointsMousePositionSampleIntervalInMs;
        public double PointsMousePositionSampleIntervalInMs
        {
            get { return pointsMousePositionSampleIntervalInMs; }
            set { SetProperty(ref pointsMousePositionSampleIntervalInMs, value); }
        }

        private bool pointsMousePositionHideCursor;
        public bool PointsMousePositionHideCursor
        {
            get { return pointsMousePositionHideCursor; }
            set { SetProperty(ref pointsMousePositionHideCursor, value); }
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

        private double keySelectionTriggerFixationDefaultCompleteTimeInMs;
        public double KeySelectionTriggerFixationDefaultCompleteTimeInMs
        {
            get { return keySelectionTriggerFixationDefaultCompleteTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationDefaultCompleteTimeInMs, value); }
        }

        private bool keySelectionTriggerFixationCompleteTimesByIndividualKey;
        public bool KeySelectionTriggerFixationCompleteTimesByIndividualKey
        {
            get { return keySelectionTriggerFixationCompleteTimesByIndividualKey; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTimesByIndividualKey, value); }
        }

        private List<KeyValueAndTimeSpanGroup> keySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups;
        public List<KeyValueAndTimeSpanGroup> KeySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups
        {
            get { return keySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups, value); }
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
                var flattenedKeySelectionTriggerFixationCompleteTimesByKeyValuesStoredSetting =
                    FromSetting(Settings.Default.KeySelectionTriggerFixationCompleteTimesByKeyValues)
                        .SelectMany(g => g.KeyValueAndTimeSpans);

                var flattenedKeySelectionTriggerFixationCompleteTimesByKeyValuesLocalValue =
                    KeySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups
                        .SelectMany(g => g.KeyValueAndTimeSpans);

                return Settings.Default.PointsSource != PointsSource
                    || (Settings.Default.TobiiEyeXProcessingLevel != TobiiEyeXProcessingLevel && PointsSource == Enums.PointsSources.TobiiEyeX)
                    || (Settings.Default.IrisbondProcessingLevel != IrisbondProcessingLevel && PointsSource == Enums.PointsSources.IrisbondDuo)
                    || (Settings.Default.PointsMousePositionSampleInterval != TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs) && PointsSource == Enums.PointsSources.MousePosition)
                    || Settings.Default.PointTtl != TimeSpan.FromMilliseconds(PointTtlInMs)
                    || Settings.Default.KeySelectionTriggerSource != KeySelectionTriggerSource
                    || (Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey != KeySelectionTriggerKeyboardKeyDownUpKey && KeySelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps)
                    || (Settings.Default.KeySelectionTriggerMouseDownUpButton != KeySelectionTriggerMouseDownUpButton && KeySelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps)
                    || (Settings.Default.KeySelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn != KeySelectionTriggerFixationResumeRequiresLockOn && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationDefaultCompleteTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || Settings.Default.KeySelectionTriggerFixationCompleteTimesByIndividualKey != KeySelectionTriggerFixationCompleteTimesByIndividualKey
                    || (flattenedKeySelectionTriggerFixationCompleteTimesByKeyValuesStoredSetting.SequenceEqual(flattenedKeySelectionTriggerFixationCompleteTimesByKeyValuesLocalValue) == false)
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
            TobiiEyeXProcessingLevel = Settings.Default.TobiiEyeXProcessingLevel;
            IrisbondProcessingLevel = Settings.Default.IrisbondProcessingLevel;
            GazeSmoothingLevel = Settings.Default.GazeSmoothingLevel;
            SmoothWhenChangingGazeTarget = Settings.Default.SmoothWhenChangingGazeTarget;
            PointsMousePositionSampleIntervalInMs = Settings.Default.PointsMousePositionSampleInterval.TotalMilliseconds;
            PointsMousePositionHideCursor = Settings.Default.PointsMousePositionHideCursor;
            PointTtlInMs = Settings.Default.PointTtl.TotalMilliseconds;
            KeySelectionTriggerSource = Settings.Default.KeySelectionTriggerSource;
            KeySelectionTriggerKeyboardKeyDownUpKey = Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey;
            KeySelectionTriggerMouseDownUpButton = Settings.Default.KeySelectionTriggerMouseDownUpButton;
            KeySelectionTriggerFixationLockOnTimeInMs = Settings.Default.KeySelectionTriggerFixationLockOnTime.TotalMilliseconds;
            KeySelectionTriggerFixationResumeRequiresLockOn = Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn;
            KeySelectionTriggerFixationDefaultCompleteTimeInMs = Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime.TotalMilliseconds;
            KeySelectionTriggerFixationCompleteTimesByIndividualKey = Settings.Default.KeySelectionTriggerFixationCompleteTimesByIndividualKey;
            KeySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups = FromSetting(Settings.Default.KeySelectionTriggerFixationCompleteTimesByKeyValues);
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
            Settings.Default.TobiiEyeXProcessingLevel = TobiiEyeXProcessingLevel;
            Settings.Default.IrisbondProcessingLevel = IrisbondProcessingLevel;
            Settings.Default.GazeSmoothingLevel = GazeSmoothingLevel;
            Settings.Default.SmoothWhenChangingGazeTarget = SmoothWhenChangingGazeTarget;
            Settings.Default.PointsMousePositionSampleInterval = TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs);
            Settings.Default.PointsMousePositionHideCursor = PointsMousePositionHideCursor;
            Settings.Default.PointTtl = TimeSpan.FromMilliseconds(PointTtlInMs);
            Settings.Default.KeySelectionTriggerSource = KeySelectionTriggerSource;
            Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey = KeySelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.KeySelectionTriggerMouseDownUpButton = KeySelectionTriggerMouseDownUpButton;
            Settings.Default.KeySelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn = KeySelectionTriggerFixationResumeRequiresLockOn;
            Settings.Default.KeySelectionTriggerFixationDefaultCompleteTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationDefaultCompleteTimeInMs);
            Settings.Default.KeySelectionTriggerFixationCompleteTimesByIndividualKey = KeySelectionTriggerFixationCompleteTimesByIndividualKey;
            Settings.Default.KeySelectionTriggerFixationCompleteTimesByKeyValues = ToSetting(KeySelectionTriggerFixationCompleteTimeInMsByKeyValueGroups);
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

        private List<KeyValueAndTimeSpanGroup> FromSetting(
            SerializableDictionaryOfTimeSpanByKeyValues dictionary)
        {
            return new List<KeyValueAndTimeSpanGroup>
            {
                new KeyValueAndTimeSpanGroup(Resources.CHANGE_KEYBOARD_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.ALPHA_1, KeyValues.Alpha1KeyboardKey, dictionary.ContainsKey(KeyValues.Alpha1KeyboardKey) ? dictionary[KeyValues.Alpha1KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ALPHA_2, KeyValues.Alpha2KeyboardKey, dictionary.ContainsKey(KeyValues.Alpha2KeyboardKey) ? dictionary[KeyValues.Alpha2KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.BACK, KeyValues.BackFromKeyboardKey, dictionary.ContainsKey(KeyValues.BackFromKeyboardKey) ? dictionary[KeyValues.BackFromKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CONVERSATION_ALPHA_1, KeyValues.ConversationAlpha1KeyboardKey, dictionary.ContainsKey(KeyValues.ConversationAlpha1KeyboardKey) ? dictionary[KeyValues.ConversationAlpha1KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CONVERSATION_ALPHA_2, KeyValues.ConversationAlpha2KeyboardKey, dictionary.ContainsKey(KeyValues.ConversationAlpha2KeyboardKey) ? dictionary[KeyValues.ConversationAlpha2KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CONVERSATION_NUMERIC_AND_SYMBOLS, KeyValues.ConversationNumericAndSymbolsKeyboardKey, dictionary.ContainsKey(KeyValues.ConversationNumericAndSymbolsKeyboardKey) ? dictionary[KeyValues.ConversationNumericAndSymbolsKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CURRENCIES_1, KeyValues.Currencies1KeyboardKey, dictionary.ContainsKey(KeyValues.Currencies1KeyboardKey) ? dictionary[KeyValues.Currencies1KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CURRENCIES_2, KeyValues.Currencies2KeyboardKey, dictionary.ContainsKey(KeyValues.Currencies2KeyboardKey) ? dictionary[KeyValues.Currencies2KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DIACRITICS_1, KeyValues.Diacritic1KeyboardKey, dictionary.ContainsKey(KeyValues.Diacritic1KeyboardKey) ? dictionary[KeyValues.Diacritic1KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DIACRITICS_2, KeyValues.Diacritic2KeyboardKey, dictionary.ContainsKey(KeyValues.Diacritic2KeyboardKey) ? dictionary[KeyValues.Diacritic2KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DIACRITICS_3, KeyValues.Diacritic3KeyboardKey, dictionary.ContainsKey(KeyValues.Diacritic3KeyboardKey) ? dictionary[KeyValues.Diacritic3KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DYNAMIC_KEYBOARD, KeyValues.DynamicKeyboardKey, dictionary.ContainsKey(KeyValues.DynamicKeyboardKey) ? dictionary[KeyValues.DynamicKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LANGUAGE_AND_VOICE_UPPER_CASE, KeyValues.LanguageKeyboardKey, dictionary.ContainsKey(KeyValues.LanguageKeyboardKey) ? dictionary[KeyValues.LanguageKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MENU, KeyValues.MenuKeyboardKey, dictionary.ContainsKey(KeyValues.MenuKeyboardKey) ? dictionary[KeyValues.MenuKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOUSE, KeyValues.MouseKeyboardKey, dictionary.ContainsKey(KeyValues.MouseKeyboardKey) ? dictionary[KeyValues.MouseKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.NUMBERS_SYMBOLS_1, KeyValues.NumericAndSymbols1KeyboardKey, dictionary.ContainsKey(KeyValues.NumericAndSymbols1KeyboardKey) ? dictionary[KeyValues.NumericAndSymbols1KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.NUMBERS_SYMBOLS_2, KeyValues.NumericAndSymbols2KeyboardKey, dictionary.ContainsKey(KeyValues.NumericAndSymbols2KeyboardKey) ? dictionary[KeyValues.NumericAndSymbols2KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.NUMBERS_SYMBOLS_3, KeyValues.NumericAndSymbols3KeyboardKey, dictionary.ContainsKey(KeyValues.NumericAndSymbols3KeyboardKey) ? dictionary[KeyValues.NumericAndSymbols3KeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.PHYSICAL_KEYS, KeyValues.PhysicalKeysKeyboardKey, dictionary.ContainsKey(KeyValues.PhysicalKeysKeyboardKey) ? dictionary[KeyValues.PhysicalKeysKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SIZE_AND_POSITION, KeyValues.SizeAndPositionKeyboardKey, dictionary.ContainsKey(KeyValues.SizeAndPositionKeyboardKey) ? dictionary[KeyValues.SizeAndPositionKeyboardKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.WEB_BROWSING, KeyValues.WebBrowsingKeyboardKey, dictionary.ContainsKey(KeyValues.WebBrowsingKeyboardKey) ? dictionary[KeyValues.WebBrowsingKeyboardKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.DOCK_ACTIONS_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.COLLAPSE_DOCK, KeyValues.CollapseDockKey, dictionary.ContainsKey(KeyValues.CollapseDockKey) ? dictionary[KeyValues.CollapseDockKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_DOCK, KeyValues.ExpandDockKey, dictionary.ContainsKey(KeyValues.ExpandDockKey) ? dictionary[KeyValues.ExpandDockKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.LANGUAGES_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.CATALAN_SPAIN, KeyValues.CatalanSpainKey, dictionary.ContainsKey(KeyValues.CatalanSpainKey) ? dictionary[KeyValues.CatalanSpainKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CROATIAN_CROATIA, KeyValues.CroatianCroatiaKey,dictionary.ContainsKey(KeyValues.CroatianCroatiaKey) ? dictionary[KeyValues.CroatianCroatiaKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.CZECH_CZECH_REPUBLIC, KeyValues.CzechCzechRepublicKey, dictionary.ContainsKey(KeyValues.CzechCzechRepublicKey) ? dictionary[KeyValues.CzechCzechRepublicKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.DANISH_DENMARK, KeyValues.DanishDenmarkKey,dictionary.ContainsKey(KeyValues.DanishDenmarkKey) ? dictionary[KeyValues.DanishDenmarkKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.DUTCH_BELGIUM, KeyValues.DutchBelgiumKey, dictionary.ContainsKey(KeyValues.DutchBelgiumKey) ? dictionary[KeyValues.DutchBelgiumKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DUTCH_NETHERLANDS, KeyValues.DutchNetherlandsKey, dictionary.ContainsKey(KeyValues.DutchNetherlandsKey) ? dictionary[KeyValues.DutchNetherlandsKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ENGLISH_CANADA, KeyValues.EnglishCanadaKey, dictionary.ContainsKey(KeyValues.EnglishCanadaKey) ? dictionary[KeyValues.EnglishCanadaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ENGLISH_UK, KeyValues.EnglishUKKey, dictionary.ContainsKey(KeyValues.EnglishUKKey) ? dictionary[KeyValues.EnglishUKKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ENGLISH_US, KeyValues.EnglishUSKey, dictionary.ContainsKey(KeyValues.EnglishUSKey) ? dictionary[KeyValues.EnglishUSKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.FINNISH_FINLAND, KeyValues.FinnishFinlandKey, dictionary.ContainsKey(KeyValues.FinnishFinlandKey) ? dictionary[KeyValues.FinnishFinlandKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.FRENCH_CANADA, KeyValues.FrenchCanadaKey, dictionary.ContainsKey(KeyValues.FrenchCanadaKey) ? dictionary[KeyValues.FrenchCanadaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.FRENCH_FRANCE, KeyValues.FrenchFranceKey, dictionary.ContainsKey(KeyValues.FrenchFranceKey) ? dictionary[KeyValues.FrenchFranceKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.GEORGIAN_GEORGIA, KeyValues.GeorgianGeorgiaKey, dictionary.ContainsKey(KeyValues.GeorgianGeorgiaKey) ? dictionary[KeyValues.GeorgianGeorgiaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.GERMAN_GERMANY, KeyValues.GermanGermanyKey, dictionary.ContainsKey(KeyValues.GermanGermanyKey) ? dictionary[KeyValues.GermanGermanyKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.GREEK_GREECE, KeyValues.GreekGreeceKey, dictionary.ContainsKey(KeyValues.GreekGreeceKey) ? dictionary[KeyValues.GreekGreeceKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.HEBREW_ISRAEL, KeyValues.HebrewIsraelKey, dictionary.ContainsKey(KeyValues.HebrewIsraelKey) ? dictionary[KeyValues.HebrewIsraelKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.HINDI_INDIA, KeyValues.HindiIndiaKey, dictionary.ContainsKey(KeyValues.HindiIndiaKey) ? dictionary[KeyValues.HindiIndiaKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.HUNGARIAN_HUNGARY, KeyValues.HungarianHungaryKey, dictionary.ContainsKey(KeyValues.HungarianHungaryKey) ? dictionary[KeyValues.HungarianHungaryKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.ITALIAN_ITALY, KeyValues.ItalianItalyKey, dictionary.ContainsKey(KeyValues.ItalianItalyKey) ? dictionary[KeyValues.ItalianItalyKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.JAPANESE_JAPAN, KeyValues.JapaneseJapanKey, dictionary.ContainsKey(KeyValues.JapaneseJapanKey) ? dictionary[KeyValues.JapaneseJapanKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.KOREAN_KOREA, KeyValues.KoreanKoreaKey, dictionary.ContainsKey(KeyValues.KoreanKoreaKey) ? dictionary[KeyValues.KoreanKoreaKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.PERSIAN_IRAN, KeyValues.PersianIranKey, dictionary.ContainsKey(KeyValues.PersianIranKey) ? dictionary[KeyValues.PersianIranKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.POLISH_POLAND, KeyValues.PolishPolandKey, dictionary.ContainsKey(KeyValues.PolishPolandKey) ? dictionary[KeyValues.PolishPolandKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.PORTUGUESE_PORTUGAL, KeyValues.PortuguesePortugalKey, dictionary.ContainsKey(KeyValues.PortuguesePortugalKey) ? dictionary[KeyValues.PortuguesePortugalKey] : (TimeSpan?) null),
                    new KeyValueAndTimeSpan(Resources.RUSSIAN_RUSSIA, KeyValues.RussianRussiaKey, dictionary.ContainsKey(KeyValues.RussianRussiaKey) ? dictionary[KeyValues.RussianRussiaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SERBIAN_SERBIA, KeyValues.SerbianSerbiaKey, dictionary.ContainsKey(KeyValues.SerbianSerbiaKey) ? dictionary[KeyValues.SerbianSerbiaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SLOVAK_SLOVAKIA, KeyValues.SlovakSlovakiaKey, dictionary.ContainsKey(KeyValues.SlovakSlovakiaKey) ? dictionary[KeyValues.SlovakSlovakiaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SLOVENIAN_SLOVENIA, KeyValues.SlovenianSloveniaKey, dictionary.ContainsKey(KeyValues.SlovenianSloveniaKey) ? dictionary[KeyValues.SlovenianSloveniaKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SPANISH_SPAIN, KeyValues.SpanishSpainKey, dictionary.ContainsKey(KeyValues.SpanishSpainKey) ? dictionary[KeyValues.SpanishSpainKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.TURKISH_TURKEY, KeyValues.TurkishTurkeyKey, dictionary.ContainsKey(KeyValues.TurkishTurkeyKey) ? dictionary[KeyValues.TurkishTurkeyKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.UKRAINIAN_UKRAINE, KeyValues.UkrainianUkraineKey, dictionary.ContainsKey(KeyValues.UkrainianUkraineKey) ? dictionary[KeyValues.UkrainianUkraineKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.URDU_PAKISTAN, KeyValues.UrduPakistanKey, dictionary.ContainsKey(KeyValues.UrduPakistanKey) ? dictionary[KeyValues.UrduPakistanKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.LOOK_TO_SCROLL_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.LOOK_TO_SCROLL_ACTIVE_KEY_GROUP_LABEL, KeyValues.LookToScrollActiveKey, dictionary.ContainsKey(KeyValues.LookToScrollActiveKey) ? dictionary[KeyValues.LookToScrollActiveKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LOOK_TO_SCROLL_BOUNDS_KEY_GROUP_LABEL, KeyValues.LookToScrollBoundsKey, dictionary.ContainsKey(KeyValues.LookToScrollBoundsKey) ? dictionary[KeyValues.LookToScrollBoundsKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LOOK_TO_SCROLL_INCREMENT_KEY_GROUP_LABEL, KeyValues.LookToScrollIncrementKey, dictionary.ContainsKey(KeyValues.LookToScrollIncrementKey) ? dictionary[KeyValues.LookToScrollIncrementKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LOOK_TO_SCROLL_MODE_KEY_GROUP_LABEL, KeyValues.LookToScrollModeKey, dictionary.ContainsKey(KeyValues.LookToScrollModeKey) ? dictionary[KeyValues.LookToScrollModeKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LOOK_TO_SCROLL_SPEED_KEY_GROUP_LABEL, KeyValues.LookToScrollSpeedKey, dictionary.ContainsKey(KeyValues.LookToScrollSpeedKey) ? dictionary[KeyValues.LookToScrollSpeedKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MISC_ACTIONS_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.ADD_TO_DICTIONARY, KeyValues.AddToDictionaryKey, dictionary.ContainsKey(KeyValues.AddToDictionaryKey) ? dictionary[KeyValues.AddToDictionaryKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ATTENTION, KeyValues.AttentionKey, dictionary.ContainsKey(KeyValues.AttentionKey) ? dictionary[KeyValues.AttentionKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.BACK_WORD, KeyValues.BackManyKey, dictionary.ContainsKey(KeyValues.BackManyKey) ? dictionary[KeyValues.BackManyKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.BACK_ONE, KeyValues.BackOneKey, dictionary.ContainsKey(KeyValues.BackOneKey) ? dictionary[KeyValues.BackOneKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RE_CALIBRATE, KeyValues.CalibrateKey, dictionary.ContainsKey(KeyValues.CalibrateKey) ? dictionary[KeyValues.CalibrateKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CLEAR, KeyValues.ClearScratchpadKey, dictionary.ContainsKey(KeyValues.ClearScratchpadKey) ? dictionary[KeyValues.ClearScratchpadKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DECREASE_OPACITY, KeyValues.DecreaseOpacityKey, dictionary.ContainsKey(KeyValues.DecreaseOpacityKey) ? dictionary[KeyValues.DecreaseOpacityKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.INCREASE_OPACITY, KeyValues.IncreaseOpacityKey, dictionary.ContainsKey(KeyValues.IncreaseOpacityKey) ? dictionary[KeyValues.IncreaseOpacityKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MINIMISE, KeyValues.MinimiseKey, dictionary.ContainsKey(KeyValues.MinimiseKey) ? dictionary[KeyValues.MinimiseKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MORE_UPPER_CASE, KeyValues.MoreKey, dictionary.ContainsKey(KeyValues.MoreKey) ? dictionary[KeyValues.MoreKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MULTI_KEY_SELECTION_UPPER_CASE, KeyValues.MultiKeySelectionIsOnKey, dictionary.ContainsKey(KeyValues.MultiKeySelectionIsOnKey) ? dictionary[KeyValues.MultiKeySelectionIsOnKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.NO, KeyValues.NoQuestionResultKey, dictionary.ContainsKey(KeyValues.NoQuestionResultKey) ? dictionary[KeyValues.NoQuestionResultKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.QUIT, KeyValues.QuitKey, dictionary.ContainsKey(KeyValues.QuitKey) ? dictionary[KeyValues.QuitKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RESTART_UPPERCASE, KeyValues.RestartKey, dictionary.ContainsKey(KeyValues.RestartKey) ? dictionary[KeyValues.RestartKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SELECT_VOICE, KeyValues.SelectVoiceKey, dictionary.ContainsKey(KeyValues.SelectVoiceKey) ? dictionary[KeyValues.SelectVoiceKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SLEEP, KeyValues.SleepKey, dictionary.ContainsKey(KeyValues.SleepKey) ? dictionary[KeyValues.SleepKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SPEAK, KeyValues.SpeakKey, dictionary.ContainsKey(KeyValues.SpeakKey) ? dictionary[KeyValues.SpeakKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.YES, KeyValues.YesQuestionResultKey, dictionary.ContainsKey(KeyValues.YesQuestionResultKey) ? dictionary[KeyValues.YesQuestionResultKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MODIFIER_KEYS_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.ALT, KeyValues.LeftAltKey, dictionary.ContainsKey(KeyValues.LeftAltKey) ? dictionary[KeyValues.LeftAltKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CTRL, KeyValues.LeftCtrlKey, dictionary.ContainsKey(KeyValues.LeftCtrlKey) ? dictionary[KeyValues.LeftCtrlKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHIFT, KeyValues.LeftShiftKey, dictionary.ContainsKey(KeyValues.LeftShiftKey) ? dictionary[KeyValues.LeftShiftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.WIN, KeyValues.LeftWinKey, dictionary.ContainsKey(KeyValues.LeftWinKey) ? dictionary[KeyValues.LeftWinKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MOUSE_DO_AT_CURRENT_LOCATION_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.LEFT_CLICK, KeyValues.MouseLeftClickKey, dictionary.ContainsKey(KeyValues.MouseLeftClickKey) ? dictionary[KeyValues.MouseLeftClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LEFT_DOUBLE_CLICK, KeyValues.MouseLeftDoubleClickKey, dictionary.ContainsKey(KeyValues.MouseLeftDoubleClickKey) ? dictionary[KeyValues.MouseLeftDoubleClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LEFT_DOWN_UP, KeyValues.MouseLeftDownUpKey, dictionary.ContainsKey(KeyValues.MouseLeftDownUpKey) ? dictionary[KeyValues.MouseLeftDownUpKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MAGNETIC_CURSOR, KeyValues.MouseMagneticCursorKey, dictionary.ContainsKey(KeyValues.MouseMagneticCursorKey) ? dictionary[KeyValues.MouseMagneticCursorKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MIDDLE_CLICK, KeyValues.MouseMiddleClickKey, dictionary.ContainsKey(KeyValues.MouseMiddleClickKey) ? dictionary[KeyValues.MouseMiddleClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MIDDLE_DOWN_UP, KeyValues.MouseMiddleDownUpKey, dictionary.ContainsKey(KeyValues.MouseMiddleDownUpKey) ? dictionary[KeyValues.MouseMiddleDownUpKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_AMOUNT_IN_PIXEL, KeyValues.MouseMoveAmountInPixelsKey, dictionary.ContainsKey(KeyValues.MouseMoveAmountInPixelsKey) ? dictionary[KeyValues.MouseMoveAmountInPixelsKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_DOWN, KeyValues.MouseMoveToBottomKey, dictionary.ContainsKey(KeyValues.MouseMoveToBottomKey) ? dictionary[KeyValues.MouseMoveToBottomKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_TO, KeyValues.MouseMoveToKey, dictionary.ContainsKey(KeyValues.MouseMoveToKey) ? dictionary[KeyValues.MouseMoveToKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_LEFT, KeyValues.MouseMoveToLeftKey, dictionary.ContainsKey(KeyValues.MouseMoveToLeftKey) ? dictionary[KeyValues.MouseMoveToLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_RIGHT, KeyValues.MouseMoveToRightKey, dictionary.ContainsKey(KeyValues.MouseMoveToRightKey) ? dictionary[KeyValues.MouseMoveToRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_UP, KeyValues.MouseMoveToTopKey, dictionary.ContainsKey(KeyValues.MouseMoveToTopKey) ? dictionary[KeyValues.MouseMoveToTopKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RIGHT_CLICK, KeyValues.MouseRightClickKey, dictionary.ContainsKey(KeyValues.MouseRightClickKey) ? dictionary[KeyValues.MouseRightClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RIGHT_DOWN_UP, KeyValues.MouseRightDownUpKey, dictionary.ContainsKey(KeyValues.MouseRightDownUpKey) ? dictionary[KeyValues.MouseRightDownUpKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MOUSE_DO_AT_POINT_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.CLICK_AND_DRAG, KeyValues.MouseDragKey, dictionary.ContainsKey(KeyValues.MouseDragKey) ? dictionary[KeyValues.MouseDragKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LEFT_CLICK, KeyValues.MouseMoveAndLeftClickKey, dictionary.ContainsKey(KeyValues.MouseMoveAndLeftClickKey) ? dictionary[KeyValues.MouseMoveAndLeftClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LEFT_DOUBLE_CLICK, KeyValues.MouseMoveAndLeftDoubleClickKey, dictionary.ContainsKey(KeyValues.MouseMoveAndLeftDoubleClickKey) ? dictionary[KeyValues.MouseMoveAndLeftDoubleClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MIDDLE_CLICK, KeyValues.MouseMoveAndMiddleClickKey, dictionary.ContainsKey(KeyValues.MouseMoveAndMiddleClickKey) ? dictionary[KeyValues.MouseMoveAndMiddleClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RIGHT_CLICK, KeyValues.MouseMoveAndRightClickKey, dictionary.ContainsKey(KeyValues.MouseMoveAndRightClickKey) ? dictionary[KeyValues.MouseMoveAndRightClickKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCROLL_DOWN, KeyValues.MouseMoveAndScrollToBottomKey, dictionary.ContainsKey(KeyValues.MouseMoveAndScrollToBottomKey) ? dictionary[KeyValues.MouseMoveAndScrollToBottomKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCROLL_LEFT, KeyValues.MouseMoveAndScrollToLeftKey, dictionary.ContainsKey(KeyValues.MouseMoveAndScrollToLeftKey) ? dictionary[KeyValues.MouseMoveAndScrollToLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCROLL_RIGHT, KeyValues.MouseMoveAndScrollToRightKey, dictionary.ContainsKey(KeyValues.MouseMoveAndScrollToRightKey) ? dictionary[KeyValues.MouseMoveAndScrollToRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCROLL_UP, KeyValues.MouseMoveAndScrollToTopKey, dictionary.ContainsKey(KeyValues.MouseMoveAndScrollToTopKey) ? dictionary[KeyValues.MouseMoveAndScrollToTopKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCROLL_AMOUNT_IN_CLICKS, KeyValues.MouseScrollAmountInClicksKey, dictionary.ContainsKey(KeyValues.MouseScrollAmountInClicksKey) ? dictionary[KeyValues.MouseScrollAmountInClicksKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MOUSE_MISC_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.MAGNIFIER, KeyValues.MouseMagnifierKey, dictionary.ContainsKey(KeyValues.MouseMagnifierKey) ? dictionary[KeyValues.MouseMagnifierKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.REPEAT_LAST, KeyValues.RepeatLastMouseActionKey, dictionary.ContainsKey(KeyValues.RepeatLastMouseActionKey) ? dictionary[KeyValues.RepeatLastMouseActionKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.MOVE_AND_RESIZE_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.EXPAND_DOWN_AND_LEFT, KeyValues.ExpandToBottomAndLeftKey, dictionary.ContainsKey(KeyValues.ExpandToBottomAndLeftKey) ? dictionary[KeyValues.ExpandToBottomAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_DOWN_AND_RIGHT, KeyValues.ExpandToBottomAndRightKey, dictionary.ContainsKey(KeyValues.ExpandToBottomAndRightKey) ? dictionary[KeyValues.ExpandToBottomAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_DOWN, KeyValues.ExpandToBottomKey, dictionary.ContainsKey(KeyValues.ExpandToBottomKey) ? dictionary[KeyValues.ExpandToBottomKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_LEFT, KeyValues.ExpandToLeftKey, dictionary.ContainsKey(KeyValues.ExpandToLeftKey) ? dictionary[KeyValues.ExpandToLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_RIGHT, KeyValues.ExpandToRightKey, dictionary.ContainsKey(KeyValues.ExpandToRightKey) ? dictionary[KeyValues.ExpandToRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_UP_AND_LEFT, KeyValues.ExpandToTopAndLeftKey, dictionary.ContainsKey(KeyValues.ExpandToTopAndLeftKey) ? dictionary[KeyValues.ExpandToTopAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_UP_AND_RIGHT, KeyValues.ExpandToTopAndRightKey, dictionary.ContainsKey(KeyValues.ExpandToTopAndRightKey) ? dictionary[KeyValues.ExpandToTopAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.EXPAND_UP, KeyValues.ExpandToTopKey, dictionary.ContainsKey(KeyValues.ExpandToTopKey) ? dictionary[KeyValues.ExpandToTopKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ADJUST_AMOUNT_IN_PIXELS, KeyValues.MoveAndResizeAdjustmentAmountKey, dictionary.ContainsKey(KeyValues.MoveAndResizeAdjustmentAmountKey) ? dictionary[KeyValues.MoveAndResizeAdjustmentAmountKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_DOWN_AND_LEFT, KeyValues.MoveToBottomAndLeftBoundariesKey, dictionary.ContainsKey(KeyValues.MoveToBottomAndLeftBoundariesKey) ? dictionary[KeyValues.MoveToBottomAndLeftBoundariesKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_DOWN_AND_LEFT, KeyValues.MoveToBottomAndLeftKey, dictionary.ContainsKey(KeyValues.MoveToBottomAndLeftKey) ? dictionary[KeyValues.MoveToBottomAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_DOWN_AND_RIGHT, KeyValues.MoveToBottomAndRightBoundariesKey, dictionary.ContainsKey(KeyValues.MoveToBottomAndRightBoundariesKey) ? dictionary[KeyValues.MoveToBottomAndRightBoundariesKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_DOWN_AND_RIGHT, KeyValues.MoveToBottomAndRightKey, dictionary.ContainsKey(KeyValues.MoveToBottomAndRightKey) ? dictionary[KeyValues.MoveToBottomAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_DOWN, KeyValues.MoveToBottomBoundaryKey, dictionary.ContainsKey(KeyValues.MoveToBottomBoundaryKey) ? dictionary[KeyValues.MoveToBottomBoundaryKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_DOWN, KeyValues.MoveToBottomKey, dictionary.ContainsKey(KeyValues.MoveToBottomKey) ? dictionary[KeyValues.MoveToBottomKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_LEFT, KeyValues.MoveToLeftBoundaryKey, dictionary.ContainsKey(KeyValues.MoveToLeftBoundaryKey) ? dictionary[KeyValues.MoveToLeftBoundaryKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_LEFT, KeyValues.MoveToLeftKey, dictionary.ContainsKey(KeyValues.MoveToLeftKey) ? dictionary[KeyValues.MoveToLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_RIGHT, KeyValues.MoveToRightBoundaryKey, dictionary.ContainsKey(KeyValues.MoveToRightBoundaryKey) ? dictionary[KeyValues.MoveToRightBoundaryKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_RIGHT, KeyValues.MoveToRightKey, dictionary.ContainsKey(KeyValues.MoveToRightKey) ? dictionary[KeyValues.MoveToRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_UP_AND_LEFT, KeyValues.MoveToTopAndLeftBoundariesKey, dictionary.ContainsKey(KeyValues.MoveToTopAndLeftBoundariesKey) ? dictionary[KeyValues.MoveToTopAndLeftBoundariesKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_UP_AND_LEFT, KeyValues.MoveToTopAndLeftKey, dictionary.ContainsKey(KeyValues.MoveToTopAndLeftKey) ? dictionary[KeyValues.MoveToTopAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_UP_AND_RIGHT, KeyValues.MoveToTopAndRightBoundariesKey, dictionary.ContainsKey(KeyValues.MoveToTopAndRightBoundariesKey) ? dictionary[KeyValues.MoveToTopAndRightBoundariesKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_UP_AND_RIGHT, KeyValues.MoveToTopAndRightKey, dictionary.ContainsKey(KeyValues.MoveToTopAndRightKey) ? dictionary[KeyValues.MoveToTopAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.JUMP_UP, KeyValues.MoveToTopBoundaryKey, dictionary.ContainsKey(KeyValues.MoveToTopBoundaryKey) ? dictionary[KeyValues.MoveToTopBoundaryKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.MOVE_UP, KeyValues.MoveToTopKey, dictionary.ContainsKey(KeyValues.MoveToTopKey) ? dictionary[KeyValues.MoveToTopKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_UP_AND_RIGHT, KeyValues.ShrinkFromBottomAndLeftKey, dictionary.ContainsKey(KeyValues.ShrinkFromBottomAndLeftKey) ? dictionary[KeyValues.ShrinkFromBottomAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_UP_AND_LEFT, KeyValues.ShrinkFromBottomAndRightKey, dictionary.ContainsKey(KeyValues.ShrinkFromBottomAndRightKey) ? dictionary[KeyValues.ShrinkFromBottomAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_UP, KeyValues.ShrinkFromBottomKey, dictionary.ContainsKey(KeyValues.ShrinkFromBottomKey) ? dictionary[KeyValues.ShrinkFromBottomKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_RIGHT, KeyValues.ShrinkFromLeftKey, dictionary.ContainsKey(KeyValues.ShrinkFromLeftKey) ? dictionary[KeyValues.ShrinkFromLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_LEFT, KeyValues.ShrinkFromRightKey, dictionary.ContainsKey(KeyValues.ShrinkFromRightKey) ? dictionary[KeyValues.ShrinkFromRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_DOWN_AND_RIGHT, KeyValues.ShrinkFromTopAndLeftKey, dictionary.ContainsKey(KeyValues.ShrinkFromTopAndLeftKey) ? dictionary[KeyValues.ShrinkFromTopAndLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_DOWN_AND_LEFT, KeyValues.ShrinkFromTopAndRightKey, dictionary.ContainsKey(KeyValues.ShrinkFromTopAndRightKey) ? dictionary[KeyValues.ShrinkFromTopAndRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SHRINK_DOWN, KeyValues.ShrinkFromTopKey, dictionary.ContainsKey(KeyValues.ShrinkFromTopKey) ? dictionary[KeyValues.ShrinkFromTopKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.PHYSICAL_KEYS_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.DOWN_ARROW, KeyValues.ArrowDownKey, dictionary.ContainsKey(KeyValues.ArrowDownKey) ? dictionary[KeyValues.ArrowDownKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.LEFT_ARROW, KeyValues.ArrowLeftKey, dictionary.ContainsKey(KeyValues.ArrowLeftKey) ? dictionary[KeyValues.ArrowLeftKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.RIGHT_ARROW, KeyValues.ArrowRightKey, dictionary.ContainsKey(KeyValues.ArrowRightKey) ? dictionary[KeyValues.ArrowRightKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.UP_ARROW, KeyValues.ArrowUpKey, dictionary.ContainsKey(KeyValues.ArrowUpKey) ? dictionary[KeyValues.ArrowUpKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.BREAK, KeyValues.BreakKey, dictionary.ContainsKey(KeyValues.BreakKey) ? dictionary[KeyValues.BreakKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.DEL, KeyValues.DeleteKey, dictionary.ContainsKey(KeyValues.DeleteKey) ? dictionary[KeyValues.DeleteKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.END, KeyValues.EndKey, dictionary.ContainsKey(KeyValues.EndKey) ? dictionary[KeyValues.EndKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.ESC, KeyValues.EscapeKey, dictionary.ContainsKey(KeyValues.EscapeKey) ? dictionary[KeyValues.EscapeKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F10, KeyValues.F10Key, dictionary.ContainsKey(KeyValues.F10Key) ? dictionary[KeyValues.F10Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F11, KeyValues.F11Key, dictionary.ContainsKey(KeyValues.F11Key) ? dictionary[KeyValues.F11Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F12, KeyValues.F12Key, dictionary.ContainsKey(KeyValues.F12Key) ? dictionary[KeyValues.F12Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F1, KeyValues.F1Key, dictionary.ContainsKey(KeyValues.F1Key) ? dictionary[KeyValues.F1Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F2, KeyValues.F2Key, dictionary.ContainsKey(KeyValues.F2Key) ? dictionary[KeyValues.F2Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F3, KeyValues.F3Key, dictionary.ContainsKey(KeyValues.F3Key) ? dictionary[KeyValues.F3Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F4, KeyValues.F4Key, dictionary.ContainsKey(KeyValues.F4Key) ? dictionary[KeyValues.F4Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F5, KeyValues.F5Key, dictionary.ContainsKey(KeyValues.F5Key) ? dictionary[KeyValues.F5Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F6, KeyValues.F6Key, dictionary.ContainsKey(KeyValues.F6Key) ? dictionary[KeyValues.F6Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F7, KeyValues.F7Key, dictionary.ContainsKey(KeyValues.F7Key) ? dictionary[KeyValues.F7Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F8, KeyValues.F8Key, dictionary.ContainsKey(KeyValues.F8Key) ? dictionary[KeyValues.F8Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.F9, KeyValues.F9Key, dictionary.ContainsKey(KeyValues.F9Key) ? dictionary[KeyValues.F9Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.HOME, KeyValues.HomeKey, dictionary.ContainsKey(KeyValues.HomeKey) ? dictionary[KeyValues.HomeKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.INS, KeyValues.InsertKey, dictionary.ContainsKey(KeyValues.InsertKey) ? dictionary[KeyValues.InsertKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.CONTEXTUAL_MENU_KEY, KeyValues.MenuKey, dictionary.ContainsKey(KeyValues.MenuKey) ? dictionary[KeyValues.MenuKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.NUM_LK, KeyValues.NumberLockKey, dictionary.ContainsKey(KeyValues.NumberLockKey) ? dictionary[KeyValues.NumberLockKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.PG_DN, KeyValues.PgDnKey, dictionary.ContainsKey(KeyValues.PgDnKey) ? dictionary[KeyValues.PgDnKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.PG_UP, KeyValues.PgUpKey, dictionary.ContainsKey(KeyValues.PgUpKey) ? dictionary[KeyValues.PgUpKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.PRNT_SCR, KeyValues.PrintScreenKey, dictionary.ContainsKey(KeyValues.PrintScreenKey) ? dictionary[KeyValues.PrintScreenKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SCRN_LK, KeyValues.ScrollLockKey, dictionary.ContainsKey(KeyValues.ScrollLockKey) ? dictionary[KeyValues.ScrollLockKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.TAB, KeyValues.TabKey, dictionary.ContainsKey(KeyValues.TabKey) ? dictionary[KeyValues.TabKey] : (TimeSpan?)null),
                }),
                new KeyValueAndTimeSpanGroup(Resources.SUGGESTION_KEYS_KEY_GROUP, new List<KeyValueAndTimeSpan>
                {
                    new KeyValueAndTimeSpan(Resources.NEXT, KeyValues.NextSuggestionsKey, dictionary.ContainsKey(KeyValues.NextSuggestionsKey) ? dictionary[KeyValues.NextSuggestionsKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.PREV, KeyValues.PreviousSuggestionsKey, dictionary.ContainsKey(KeyValues.PreviousSuggestionsKey) ? dictionary[KeyValues.PreviousSuggestionsKey] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_1, KeyValues.Suggestion1Key, dictionary.ContainsKey(KeyValues.Suggestion1Key) ? dictionary[KeyValues.Suggestion1Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_2, KeyValues.Suggestion2Key, dictionary.ContainsKey(KeyValues.Suggestion2Key) ? dictionary[KeyValues.Suggestion2Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_3, KeyValues.Suggestion3Key, dictionary.ContainsKey(KeyValues.Suggestion3Key) ? dictionary[KeyValues.Suggestion3Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_4, KeyValues.Suggestion4Key, dictionary.ContainsKey(KeyValues.Suggestion4Key) ? dictionary[KeyValues.Suggestion4Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_5, KeyValues.Suggestion5Key, dictionary.ContainsKey(KeyValues.Suggestion5Key) ? dictionary[KeyValues.Suggestion5Key] : (TimeSpan?)null),
                    new KeyValueAndTimeSpan(Resources.SUGGESTION_6, KeyValues.Suggestion6Key, dictionary.ContainsKey(KeyValues.Suggestion6Key) ? dictionary[KeyValues.Suggestion6Key] : (TimeSpan?)null),
                })
            };
        }

        private SerializableDictionaryOfTimeSpanByKeyValues ToSetting(
            IEnumerable<KeyValueAndTimeSpanGroup> groups)
        {
            var dictionary = new SerializableDictionaryOfTimeSpanByKeyValues();
            groups.SelectMany(g => g.KeyValueAndTimeSpans)
                .Where(kvats => kvats.TimeSpanTotalMilliseconds != null)
                .ToList()
                .ForEach(kvats => dictionary.Add(kvats.KeyValue, TimeSpan.FromMilliseconds(kvats.TimeSpanTotalMilliseconds.Value)));
            return dictionary;
        }

        #endregion
    }
}
