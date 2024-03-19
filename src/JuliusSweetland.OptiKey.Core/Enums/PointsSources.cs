// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
namespace JuliusSweetland.OptiKey.Enums
{
    public enum PointsSources
    {
        [System.Obsolete("Not supported in v4+", true)]
        Alienware17,

        GazeTracker,
        IrisbondDuo,
        IrisbondHiru,
        MousePosition,
        TouchScreenPosition,

        [System.Obsolete("Not supported in v4+", true)]
        SteelseriesSentry,

        TheEyeTribe,

        [System.Obsolete("Not supported in v4+", true)]
        TobiiEyeTracker4C,
        [System.Obsolete("Not supported in v4+", true)]
        TobiiEyeTracker5,
        [System.Obsolete("Not supported in v4+", true)]
        TobiiEyeX,
        [System.Obsolete("Not supported in v4+", true)]
        TobiiRex,

        TobiiPcEyeGo,
        TobiiPcEyeGoPlus,
        TobiiPcEye5,
        TobiiPcEyeMini,
        TobiiX2_30,
        TobiiX2_60,

        [System.Obsolete("Not supported in v4+", true)]
        VisualInteractionMyGaze
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this PointsSources pointSource)
        {
            switch (pointSource)
            {
                case PointsSources.GazeTracker: return Resources.GAZE_TRACKER;
                case PointsSources.IrisbondDuo: return Resources.IRISBOND_DUO;
                case PointsSources.IrisbondHiru: return Resources.IRISBOND_HIRU;
                case PointsSources.MousePosition: return Resources.MOUSE_POSITION;
                case PointsSources.TouchScreenPosition: return Resources.TOUCHSCREEN_POSITION;
                
                case PointsSources.TheEyeTribe: return Resources.THE_EYE_TRIBE;
                case PointsSources.TobiiPcEyeGo: return Resources.TOBII_PCEYE_GO;
                case PointsSources.TobiiPcEyeGoPlus: return Resources.TOBII_PCEYE_GO_PLUS;
                case PointsSources.TobiiPcEye5: return Resources.TOBII_PCEYE_5;
                case PointsSources.TobiiPcEyeMini: return Resources.TOBII_PCEYE_MINI;
                case PointsSources.TobiiX2_30: return Resources.TOBII_X2_30;
                case PointsSources.TobiiX2_60: return Resources.TOBII_X2_60;
            }

            return pointSource.ToString();
        }

        public static string ToExtendedDescription(this PointsSources pointSource)
        {
            switch (pointSource)
            {
                case PointsSources.GazeTracker: return Resources.GAZE_TRACKER_INFO;
                case PointsSources.IrisbondDuo: return Resources.IRISBOND_DUO_INFO;
                case PointsSources.IrisbondHiru: return Resources.IRISBOND_HIRU_INFO;
                case PointsSources.MousePosition: return Resources.MOUSE_POSITION_INFO;
                case PointsSources.TheEyeTribe: return "";
                case PointsSources.TobiiPcEyeGo: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEyeGoPlus: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEye5: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiPcEyeMini: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiX2_30: return Resources.TOBII_ASSISTIVE_INFO;
                case PointsSources.TobiiX2_60: return Resources.TOBII_ASSISTIVE_INFO;
            }

            return pointSource.ToString();
        }
    }

}
