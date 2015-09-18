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
                case PointsSources.GazeTracker: return Resources.GazeTracker;
                case PointsSources.TheEyeTribe: return Resources.TheEyeTribe;
                case PointsSources.MousePosition: return Resources.MousePosition;
                case PointsSources.TobiiEyeX: return Resources.TobiiEyeX;
                case PointsSources.TobiiRex: return Resources.TobiiREX;
                case PointsSources.TobiiPcEyeGo: return Resources.TobiiPCEyeGo;
            }

            return pointSource.ToString();
        }
    }
}
