// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
using NUnit.Framework;
using System;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    [TestFixture]
    public class WhenChangesRequireRestart : ManagementViewModelTestBase
    {
        protected bool Result { get; private set; }

        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.Debug = false;
            ManagementViewModel.VisualsViewModel.ConversationOnlyMode = false;
            Settings.Default.ConversationOnlyMode = false;
        }

        [Test]
        public void GivenOtherViewModelRequiresRestart()
        {
            Settings.Default.Debug = true;
            Assert.That(ManagementViewModel.ChangesRequireRestart, Is.True);
        }

        [Test]
        public void GivenPointingAndSelectingViewModelRequiresRestart()
        {
            Settings.Default.MultiKeySelectionMaxDuration = TimeSpan.FromSeconds(42);
            Assert.That(ManagementViewModel.ChangesRequireRestart, Is.True);
        }

        [Test]
        public void GivenVisualsViewModelRequiresRestart()
        {
            ManagementViewModel.VisualsViewModel.ConversationOnlyMode = false;
            Settings.Default.ConversationOnlyMode = true;
            Assert.That(ManagementViewModel.ChangesRequireRestart, Is.True);
        }
    }
}
