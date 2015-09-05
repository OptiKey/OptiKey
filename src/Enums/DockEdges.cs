using JuliusSweetland.OptiKey.Native.Enums;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum DockEdges
    {
        Top,
        Left,
        Bottom,
        Right
    }

    public static partial class EnumExtensions
    {
        public static AppBarEdge ToAppBarEdge(this DockEdges dockPosition)
        {
            switch (dockPosition)
            {
                case DockEdges.Left: return AppBarEdge.Left;
                case DockEdges.Bottom: return AppBarEdge.Bottom;
                case DockEdges.Right: return AppBarEdge.Right;
                default: return AppBarEdge.Top;
            }
        }
    }
}
