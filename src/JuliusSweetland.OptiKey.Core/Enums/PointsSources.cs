// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
namespace JuliusSweetland.OptiKey.Enums
{
    public enum PointsSources
    {
        Alienware17,
        GazeTracker,
        IrisbondDuo,
        IrisbondHiru,
        MousePosition,
        SteelseriesSentry,
        TheEyeTribe,
        TobiiEyeTracker4C,
        TobiiEyeTracker5,
        TobiiEyeX,
        TobiiRex,
        TobiiPcEyeGo,
        TobiiPcEyeGoPlus,
        TobiiPcEye5,
        TobiiPcEyeMini,
        TobiiX2_30,
        TobiiX2_60,
        VisualInteractionMyGaze
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this PointsSources pointSource)
        {
            switch (pointSource)
            {
                case PointsSources.Alienware17: return Resources.ALIENWARE_17;
                case PointsSources.GazeTracker: return Resources.GAZE_TRACKER;
                case PointsSources.IrisbondDuo: return Resources.IRISBOND_DUO;
                case PointsSources.IrisbondHiru: return Resources.IRISBOND_HIRU;
                case PointsSources.MousePosition: return Resources.MOUSE_POSITION;
                case PointsSources.SteelseriesSentry: return Resources.STEELSERIES_SENTRY;
                case PointsSources.TheEyeTribe: return Resources.THE_EYE_TRIBE;
                case PointsSources.TobiiEyeTracker4C: return Resources.TOBII_EYE_TRACKER_4C;
                case PointsSources.TobiiEyeTracker5: return Resources.TOBII_EYE_TRACKER_5;
                case PointsSources.TobiiEyeX: return Resources.TOBII_EYEX;
                case PointsSources.TobiiRex: return Resources.TOBII_REX;
                case PointsSources.TobiiPcEyeGo: return Resources.TOBII_PCEYE_GO;
                case PointsSources.TobiiPcEyeGoPlus: return Resources.TOBII_PCEYE_GO_PLUS;
                case PointsSources.TobiiPcEye5: return Resources.TOBII_PCEYE_5;
                case PointsSources.TobiiPcEyeMini: return Resources.TOBII_PCEYE_MINI;
                case PointsSources.TobiiX2_30: return Resources.TOBII_X2_30;
                case PointsSources.TobiiX2_60: return Resources.TOBII_X2_60;
                case PointsSources.VisualInteractionMyGaze: return Resources.VI_MYGAZE;
            }

            return pointSource.ToString();
        }

        public static string ToExtendedDescription(this PointsSources pointSource)
        {
            switch (pointSource)
            {
                case PointsSources.Alienware17: return Resources.ALIENWARE_17_INFO;
                case PointsSources.GazeTracker: return Resources.GAZE_TRACKER_INFO;
                case PointsSources.IrisbondDuo: return Resources.IRISBOND_DUO_INFO;
                case PointsSources.IrisbondHiru: return Resources.IRISBOND_HIRU_INFO;
                case PointsSources.MousePosition: return Resources.MOUSE_POSITION_INFO;
                case PointsSources.SteelseriesSentry: return Resources.TOBII_EYEX_INFO;
                case PointsSources.TheEyeTribe: return "";
                case PointsSources.TobiiEyeTracker4C: return Resources.TOBII_EYEX_INFO;
                case PointsSources.TobiiEyeTracker5: return Resources.TOBII_EYEX_INFO;
                case PointsSources.TobiiEyeX: return Resources.TOBII_EYEX_INFO;
                case PointsSources.TobiiRex: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEyeGo: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEyeGoPlus: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEye5: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEyeMini: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiX2_30: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiX2_60: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.VisualInteractionMyGaze: return Resources.VI_MYGAZE_INFO;
            }

            return pointSource.ToString();
        }
    }

}
