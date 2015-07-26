using JuliusSweetland.OptiKey.Native.Enums;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum DockPositions
    {
        Top,
        Left,
        Bottom,
        Right
    }

    public static partial class EnumExtensions
    {
        public static AppBarEdge ToAppBarEdge(this DockPositions dockPosition)
        {
            switch (dockPosition)
            {
                case DockPositions.Left: return AppBarEdge.Left;
                case DockPositions.Bottom: return AppBarEdge.Bottom;
                case DockPositions.Right: return AppBarEdge.Right;
                default: return AppBarEdge.Top;
            }
        }
    }
}
