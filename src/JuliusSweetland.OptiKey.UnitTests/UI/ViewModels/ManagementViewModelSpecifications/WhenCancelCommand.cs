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
        protected bool IsWarningRaised { get; private set; }
        protected bool IsCanceled { get; private set; }
        protected bool confirm { get; set; }

        protected override void Arrange()
        {
            base.Arrange();
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
        public void ThenWindowShouldBeClosed()
        {
            Assert.IsTrue(IsWindowClosed);
        }
    }

    [TestFixture]
    public class WhenCancelCommandStay : WhenCancelCommand
    {
        protected override void Arrange()
        {
            base.confirm = false;
            base.Arrange();
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldStay()
        {
            Assert.IsFalse(IsWindowClosed);
        }
    }
}
