using NUnit.Framework;
using System;

namespace JuliusSweetland.OptiKey.UnitTests
{
    public class TestBase
    {
        protected Exception Exception { get; private set; }

        [SetUp]
        public void Init()
        {
            Arrange();
            Act();
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
