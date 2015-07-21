namespace JuliusSweetland.OptiKey.Enums
{
    public enum MinimisePositions
    {
        TopLeftCorner, 
        TopEdge,
        TopRightCorner,
        LeftEdge,
        BottomLeftCorner,
        BottomEdge,
        BottomRightCorner,
        RightEdge
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this MinimisePositions minimisePositions)
        {
            switch (minimisePositions)
            {
                case MinimisePositions.TopLeftCorner: return "Top left corner";
                case MinimisePositions.TopEdge: return "Top edge";
                case MinimisePositions.TopRightCorner: return "Top right corner";
                case MinimisePositions.LeftEdge: return "Left edge";
                case MinimisePositions.BottomLeftCorner: return "Bottom left corner";
                case MinimisePositions.BottomEdge: return "Bottom edge";
                case MinimisePositions.BottomRightCorner: return "Bottom right corner";
                case MinimisePositions.RightEdge: return "Right edge";
            }

            return minimisePositions.ToString();
        }
    }
}
