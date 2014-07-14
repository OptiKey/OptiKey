using System.Windows;

namespace JuliusSweetland.ETTA.Model
{
    public struct PointAndKeyValue
    {
        public PointAndKeyValue(Point point, KeyValue? keyValue)
            : this()
        {
            Point = point;
            KeyValue = keyValue;
        }

        public Point Point { get; private set; }
        public KeyValue? KeyValue { get; private set; }

        /// <summary>
        /// Call through to KeyValue.Value.Letter
        /// </summary>
        public char? Letter
        {
            get { return KeyValue != null && KeyValue.Value.Letter != null ? KeyValue.Value.Letter : null; }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})=>'{2}'", Point.X, Point.Y, KeyValue);
        }
    }
}
