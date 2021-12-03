// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
using NUnit.Framework;
using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    public abstract class WhenOkCommand : ManagementViewModelTestBase
    {
        protected Window Window { get; private set; }
        protected bool IsWindowClosed { get; private set; }
        protected bool confirm { get; set; }
        protected bool changed { get; set; }

        protected const string defaultCommuniKatePagesetLocation = "./Resources/CommuniKate/pageset.obz";
        protected const string defaultMaryTTSLocation = "";

        protected override void Arrange()
        {
            base.Arrange();

            if (changed)
            {
                base.ManagementViewModel.FeaturesViewModel.CommuniKatePagesetLocation = "new location";
                base.ManagementViewModel.SoundsViewModel.MaryTTSLocation = "new location";
            }

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
            ManagementViewModel.OkCommand.Execute(Window);
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartRequired : WhenOkCommand
    {
        protected override void Arrange()
        {
            changed = true;
            confirm = false;
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldNotBeClosedOrSaved()
        {
            Assert.Multiple(() =>
            {
                Assert.That(IsWindowClosed, Is.False);

                // settings still use default values -- not closed
                Assert.That(defaultCommuniKatePagesetLocation, Is.EqualTo(Settings.Default.CommuniKatePagesetLocation));
                Assert.That(defaultMaryTTSLocation, Is.EqualTo(Settings.Default.MaryTTSLocation));
            });            
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartNotRequired : WhenOkCommand
    {
        protected override void Arrange()
        {
            changed = false;
            confirm = false;  // since no change is made, no confirmation is actually prompted, so doesn't matter what's here.
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldBeClosedWithNoChanges()
        {
            Assert.Multiple(() =>
            {
                Assert.That(IsWindowClosed, Is.True);

                // settings still use default values -- no changes made
                Assert.That(defaultCommuniKatePagesetLocation, Is.EqualTo(Settings.Default.CommuniKatePagesetLocation));
                Assert.That(defaultMaryTTSLocation, Is.EqualTo(Settings.Default.MaryTTSLocation));
            });            
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartNotRequiredAndNotChanged : WhenOkCommand
    {
        protected override void Arrange()
        {
            Settings.Default.Debug = true;

            changed = false;
            confirm = true;  // since no restart change is made, no confirmation is actually prompted, so doesn't matter what's here.
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldBeClosedWithNoCoreChanges()
        {
            Assert.Multiple(() =>
            {
                Assert.That(IsWindowClosed, Is.True);

                // core settings not being changed updated -- still use default values
                Assert.That(defaultCommuniKatePagesetLocation, Is.EqualTo(Settings.Default.CommuniKatePagesetLocation));
                Assert.That(defaultMaryTTSLocation, Is.EqualTo(Settings.Default.MaryTTSLocation));
            });            
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartRequiredAndSaved : WhenOkCommand
    {
        protected override void Arrange()
        {
            new Application();

            changed = true;
            confirm = true;
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldRestartAndSaved()
        {
            Assert.Multiple(() =>
            {
                Assert.That(IsWindowClosed, Is.False);

                // settings being updated -- using new values
                Assert.That("new location", Is.EqualTo(Settings.Default.CommuniKatePagesetLocation));
                Assert.That("new location", Is.EqualTo(Settings.Default.MaryTTSLocation));
            });            
        }
    }
}
