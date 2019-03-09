// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
using NUnit.Framework;
using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    [TestFixture]
    public abstract class WhenCancelCommand : ManagementViewModelTestBase
    {
        protected Window Window { get; private set; }
        protected bool IsWindowClosed { get; private set; }
        protected bool confirm { get; set; }

        protected const string defaultCommuniKatePagesetLocation = "./Resources/CommuniKate/pageset.obz";
        protected const string defaultMaryTTSLocation = "";

        protected override void Arrange()
        {
            base.Arrange();

            base.ManagementViewModel.WordsViewModel.CommuniKatePagesetLocation = "new location";
            base.ManagementViewModel.SoundsViewModel.MaryTTSLocation = "new location";

            base.ManagementViewModel.ConfirmationRequest.Raised += (s, e) =>
            {
                Confirmation context = e.Context as Confirmation;
                context.Confirmed = confirm;
                e.Callback();
            };

            Window = new Window();
            Window.Closed += (s, e) => IsWindowClosed = true;
        }

        protected override void Act()
        {
            ManagementViewModel.CancelCommand.Execute(Window);
        }
    }

    [TestFixture]
    public class WhenCancelCommandDismissed : WhenCancelCommand
    {
        protected override void Arrange()
        {
            base.confirm = true;
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldBeClosedWithoutSaving()
        {
            Assert.IsTrue(IsWindowClosed);
            Assert.AreEqual(Settings.Default.CommuniKatePagesetLocation, defaultCommuniKatePagesetLocation);
            Assert.AreEqual(Settings.Default.MaryTTSLocation, defaultMaryTTSLocation);
        }
    }    
}
