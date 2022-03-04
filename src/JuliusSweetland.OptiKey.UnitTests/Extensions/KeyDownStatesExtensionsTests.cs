// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Extensions
{
    [TestFixture]
    public class KeyDownStatesExtensionsTests
    {

        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }

        [Test]
        public void TestIsDownOrLockedDown()
        {
            Assert.Multiple(() =>
            {
                Assert.That(KeyDownStates.Down.IsDownOrLockedDown(), Is.True);
                Assert.That(KeyDownStates.LockedDown.IsDownOrLockedDown(), Is.True);
                Assert.That(KeyDownStates.Up.IsDownOrLockedDown(), Is.False);
            });            
        }


    }
}
