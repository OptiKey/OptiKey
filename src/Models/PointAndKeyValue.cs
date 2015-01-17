using System.Windows;

namespace JuliusSweetland.OptiKey.Models
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

        public bool StringIsLetter
        {
            get { return KeyValue != null && KeyValue.Value.StringIsLetter; }
        }

        public string String
        {
            get { return KeyValue != null ? KeyValue.Value.String : null; }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})=>'{2}'", Point.X, Point.Y, KeyValue);
        }
    }
}
