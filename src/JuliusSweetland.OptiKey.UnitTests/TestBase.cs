// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using NUnit.Framework;
using System;
using JuliusSweetland.OptiKey.UnitTests.Properties;

namespace JuliusSweetland.OptiKey.UnitTests
{
    public class TestBase
    {
        protected Exception Exception { get; private set; }

        [SetUp]
        public void Init()
        {
            Settings.Initialise();
            Arrange();
            Act();
        }

        [TearDown]
        public void TearDown()
        {
            Settings.Default.Reset();
        }

        protected virtual void Arrange()
        {
        }

        protected virtual void Act()
        {
        }

        protected virtual void ExpectException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex, "Exception expected");
                Exception = ex;
            }
        }
    }
}
