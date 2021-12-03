// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class DoubleExtensionsTests
    {
        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }

        [Test]
        public void TestCoerceToUpperLimit()
        {
            Assert.Multiple(() =>
            {
                Assert.That(100.5.CoerceToUpperLimit(10), Is.EqualTo(10));
                Assert.That(50.5.CoerceToUpperLimit(100), Is.EqualTo(50.5));
            });            
        }

        [Test]
        public void TestCoerceToLowerLimit()
        {
            Assert.Multiple(() =>
            {
                Assert.That(100.5.CoerceToLowerLimit(10), Is.EqualTo(100.5));
                Assert.That(50.5.CoerceToLowerLimit(100), Is.EqualTo(100));
            });
        }

    }
}
