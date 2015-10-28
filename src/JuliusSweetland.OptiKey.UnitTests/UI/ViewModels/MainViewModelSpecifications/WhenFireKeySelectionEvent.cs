using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    [TestFixture]
    public class WhenFireKeySelectionEventGivenKeySelectionEventHandler : MainViewModelTestBase
    {
        protected KeyValue KeyValue { get { return new KeyValue { FunctionKey = FunctionKeys.Break, String = "Coffee" }; } }

        protected override void Act()
        {
            MainViewModel.FireKeySelectionEvent(KeyValue);
        }

        [Test]
        public void ThenKeySelectionEventHandlerShouldBeTriggered()
        {
            Assert.IsTrue(IsKeySelectionEventHandlerCalled);
        }
    }
}
