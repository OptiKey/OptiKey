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
                case PointsSources.GazeTracker: return "Gaze Tracker";
                case PointsSources.TheEyeTribe: return "The Eye Tribe";
                case PointsSources.MousePosition: return "Mouse position";
                case PointsSources.TobiiEyeX: return "Tobii EyeX";
                case PointsSources.TobiiRex: return "Tobii REX";
                case PointsSources.TobiiPcEyeGo: return "Tobii PCEye Go";
            }

            return pointSource.ToString();
        }
    }
}
