// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Native.Common.Enums;

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

        public static MinimisedEdges ToMinimisedEdge(this DockEdges dockPosition)
        {
            switch (dockPosition)
            {
                case DockEdges.Left: return MinimisedEdges.Left;
                case DockEdges.Bottom: return MinimisedEdges.Bottom;
                case DockEdges.Right: return MinimisedEdges.Right;
                default: return MinimisedEdges.Top;
            }
        }
    }
}
