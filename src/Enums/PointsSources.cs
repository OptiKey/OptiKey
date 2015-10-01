using JuliusSweetland.OptiKey.Properties;
namespace JuliusSweetland.OptiKey.Enums
{
    public enum PointsSources
    {
        GazeTracker,
        TheEyeTribe,
        MousePosition,
        TobiiEyeX,
        TobiiRex,
        TobiiPcEyeGo
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this PointsSources pointSource)
        {
            switch (pointSource)
            {
                case PointsSources.GazeTracker: return Resources.GAZE_TRACKER;
                case PointsSources.TheEyeTribe: return Resources.THE_EYE_TRIBE;
                case PointsSources.MousePosition: return Resources.MOUSE_POSITION;
                case PointsSources.TobiiEyeX: return Resources.TOBII_EYEX;
                case PointsSources.TobiiRex: return Resources.TOBII_REX;
                case PointsSources.TobiiPcEyeGo: return Resources.TOBII_PCEYE_GO;
            }

            return pointSource.ToString();
        }
    }
}
