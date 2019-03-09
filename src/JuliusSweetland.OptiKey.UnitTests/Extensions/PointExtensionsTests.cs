// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.OptiKey.Extensions;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class PointExtensionsTests
    {
        [Test]
        public void TestCalculateCentrePoint()
        {
            var points = new List<Point>
            {
                new Point(1.5, 1),
                new Point(5, 5.1),
                new Point(10, 10.9)
            };

            var centrePoint = points.CalculateCentrePoint();
            Assert.AreEqual(6, centrePoint.X);
            Assert.AreEqual(6, centrePoint.Y);
        }

    }
}
