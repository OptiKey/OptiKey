using NUnit.Framework;
using System.Windows;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    [TestFixture]
    public class WhenCancelCommand : ManagementViewModelTestBase
    {
        protected Window Window { get; private set; }
        protected bool IsWindowClosed { get; private set; }

        protected override void Arrange()
        {
            base.Arrange();
            Window = new Window();
            Window.Closed += (s, e) => IsWindowClosed = true;
        }

        protected override void Act()
        {
            ManagementViewModel.CancelCommand.Execute(Window);
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldBeClosed()
        {
            Assert.IsTrue(IsWindowClosed);
        }
    }
}
