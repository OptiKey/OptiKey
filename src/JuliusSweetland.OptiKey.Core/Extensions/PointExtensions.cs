// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class PointExtensions
    {
        public static Point CalculateCentrePoint(this List<Point> points)
        {
            return new Point(
                Math.Round(points.Average(p => p.X), MidpointRounding.AwayFromZero), 
                Math.Round(points.Average(p => p.Y), MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Convert a point to a PointAndKeyValue (if one can be mapped from the supplied pointToKeyValueMap).
        /// N.B. Null will be returned if the point supplied is null.
        /// </summary>
        public static PointAndKeyValue ToPointAndKeyValue(this Point? point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            if (point == null)
            {
                return null;
            }

            return new PointAndKeyValue(point.Value, point.Value.ToKeyValue(pointToKeyValueMap));
        }

        public static KeyValue ToKeyValue(this Point point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            Rect? keyRect = pointToKeyValueMap != null
                ? pointToKeyValueMap.Keys.FirstOrDefault(r => r.Contains(point))
                : (Rect?)null;

            return keyRect != null && pointToKeyValueMap.ContainsKey(keyRect.Value)
                ? pointToKeyValueMap[keyRect.Value]
                : (KeyValue)null;
        }
    }
}
