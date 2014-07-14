using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using JuliusSweetland.ETTA.Model;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class PointExtensions
    {
        public static Point CalculateCentrePoint(this List<Point> points)
        {
            return new Point(Convert.ToInt32(points.Average(p => p.X)), Convert.ToInt32(points.Average(p => p.Y)));
        }

        public static KeyValue? ToKeyValue(this Point point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            Rect? keyRect = pointToKeyValueMap != null
                ? pointToKeyValueMap.Keys.FirstOrDefault(r => r.Contains(point))
                : (Rect?) null;

            return keyRect != null && pointToKeyValueMap.ContainsKey(keyRect.Value)
                ? pointToKeyValueMap[keyRect.Value]
                : (KeyValue?) null;
        }

        public static PointAndKeyValue? ToPointAndKeyValue(this Point? point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            if (point == null)
            {
                return null;
            }

            Rect? keyRect = pointToKeyValueMap != null
                ? pointToKeyValueMap.Keys.FirstOrDefault(r => r.Contains(point.Value))
                : (Rect?)null;

            var keyValue = keyRect != null && pointToKeyValueMap.ContainsKey(keyRect.Value)
                ? pointToKeyValueMap[keyRect.Value]
                : (KeyValue?)null;

            return new PointAndKeyValue(point.Value, keyValue);
        }
    }
}
