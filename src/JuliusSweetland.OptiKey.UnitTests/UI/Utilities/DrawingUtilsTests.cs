// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.Utilities;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.Utilities
{
    [TestFixture]
    public class DrawingUtilsTests
    {
        private static void AssertHsl2RbgColor(double h, double s, double l, int expectedR, int expectedG, int expectedB)
        {
            var color = DrawingUtils.HSL2RGB(h, s, l);

            Assert.Multiple(() =>
            {
                Assert.That(color.R, Is.EqualTo(expectedR), "Expected R + " + expectedR + " but was " + color.R);
                Assert.That(color.G, Is.EqualTo(expectedG), "Expected G: " + expectedG + " but was " + color.G);
                Assert.That(color.B, Is.EqualTo(expectedB), "Expected B: " + expectedB + " but was " + color.B);
            });
        }

        [Test]
        public void TestHslRgb()
        {
            //DrawingUtils.HSL2RGB really does no error checking. 
            //These test are here ot catch if our conversion logic changes.
            AssertHsl2RbgColor(0, 0, 0, 0, 0, 0);
            AssertHsl2RbgColor(.25, .25, .25, 64, 80, 48);
            AssertHsl2RbgColor(.01, .01, .01, 3, 3, 3);
            AssertHsl2RbgColor(.25, .5, .75, 191, 223, 159);
            AssertHsl2RbgColor(.34, .48, .16, 21, 60, 23);
            AssertHsl2RbgColor(.5, .5, .5, 64, 191, 191);
            AssertHsl2RbgColor(.7, 0, .3, 76, 76, 76);
            AssertHsl2RbgColor(.9, .5, .5, 191, 64, 140);
            AssertHsl2RbgColor(1, 1, 1, 255, 255, 255);
        }
    }
}