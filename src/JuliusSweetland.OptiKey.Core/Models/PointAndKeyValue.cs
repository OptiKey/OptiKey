// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;

namespace JuliusSweetland.OptiKey.Models
{
    public class PointAndKeyValue
    {
        public PointAndKeyValue(Point point, KeyValue keyValue)
        {
            Point = point;
            KeyValue = keyValue;
        }

        public Point Point { get; private set; }
        public KeyValue KeyValue { get; private set; }

        public bool StringIsLetter
        {
            get { return KeyValue != null && KeyValue.StringIsLetter; }
        }

        public string String
        {
            get { return KeyValue != null ? KeyValue.String : null; }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})='{2}'", Point.X, Point.Y, KeyValue);
        }
    }
}
