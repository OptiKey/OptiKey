// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using Moq;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    public abstract class WhenSetSelectionMode : MainViewModelTestBase
    {
        protected SelectionModes OriginalValue { get; set; }
        protected SelectionModes NewValue { get; set; }

        protected override void Arrange()
        {
            base.Arrange();

            OriginalValue = SelectionModes.Keys;
            NewValue = SelectionModes.SinglePoint;

            MainViewModel.SelectionMode = OriginalValue;
            InputService.ResetCalls();
        }

        protected override void Act()
        {
            MainViewModel.SelectionMode = NewValue;
        }
    }

    [TestFixture]
    public class WhenSetSelectionModeGivenValueNotChanged : WhenSetSelectionMode
    {
        protected override void Arrange()
        {
            base.Arrange();
            NewValue = OriginalValue;
        }

        [Test]
        public void ThenSelectionProgressShouldNotBeReset()
        {
            KeyStateService.VerifyGet(s => s.KeySelectionProgress, Times.Never());
        }

        [Test]
        public void ThenSelectionModeOnInputServiceShouldNotBeSet()
        {
            InputService.VerifySet(s => s.SelectionMode = NewValue, Times.Never());
        }
    }

    [TestFixture]
    public class WhenSetSelectionModeGivenValueChanged : WhenSetSelectionMode
    {
        [Test]
        public void ThenSelectionProgressShouldBeReset()
        {
            Assert.That(MainViewModel.PointSelectionProgress, Is.Null);
            KeyStateService.VerifyGet(s => s.KeySelectionProgress, Times.Once());
        }

        [Test]
        public void ThenSelectionModeOnInputServiceShouldBeSet()
        {
            InputService.VerifySet(s => s.SelectionMode = NewValue, Times.Once());
        }
    }
}
