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
            Assert.IsTrue(ManagementViewModel.ChangesRequireRestart);
        }

        [Test]
        public void GivenPointingAndSelectingViewModelRequiresRestart()
        {
            Settings.Default.MultiKeySelectionMaxDuration = TimeSpan.FromSeconds(42);
            Assert.IsTrue(ManagementViewModel.ChangesRequireRestart);
        }

        [Test]
        public void GivenVisualsViewModelRequiresRestart()
        {
            ManagementViewModel.VisualsViewModel.ConversationOnlyMode = false;
            Settings.Default.ConversationOnlyMode = true;
            Assert.IsTrue(ManagementViewModel.ChangesRequireRestart);
        }
    }
}
