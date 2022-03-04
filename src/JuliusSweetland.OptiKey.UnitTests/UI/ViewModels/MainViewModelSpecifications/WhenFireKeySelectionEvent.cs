// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    [TestFixture]
    public class WhenFireKeySelectionEventGivenKeySelectionEventHandler : MainViewModelTestBase
    {
        protected KeyValue KeyValue { get { return new KeyValue(FunctionKeys.Break); } }

        protected override void Act()
        {
            MainViewModel.FireKeySelectionEvent(KeyValue);
        }

        [Test]
        public void ThenKeySelectionEventHandlerShouldBeTriggered()
        {
            Assert.That(IsKeySelectionEventHandlerCalled, Is.True);
        }
    }
}
