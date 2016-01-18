using System.Drawing;
using JuliusSweetland.OptiKey.UI.Utilities;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.Utilities
{
    [TestFixture]
    public class ColourRgbTests
    {
        [Test]
        public void TestColourRgbConstructor()
        {
            var colourRgb = new ColourRgb(155, 75, 42);
            Assert.AreEqual(155, colourRgb.R);
            Assert.AreEqual(75, colourRgb.G);
            Assert.AreEqual(42, colourRgb.B);
        }

        [Test]
        public void TestColourRgbExplicitFromColor()
        {
            var color = Color.FromArgb(155, 75, 42);
            var colourRgb = (ColourRgb) color;
            Assert.AreEqual(155, colourRgb.R);
            Assert.AreEqual(75, colourRgb.G);
            Assert.AreEqual(42, colourRgb.B);
        }

        [Test]
        public void TestColourRgbImplicitOperatorToColor()
        {
            var colourRgb = new ColourRgb(155, 75, 42);
            Color color = colourRgb;
            Assert.AreEqual(155, color.R);
            Assert.AreEqual(75, color.G);
            Assert.AreEqual(42, color.B);
        }
    }
}