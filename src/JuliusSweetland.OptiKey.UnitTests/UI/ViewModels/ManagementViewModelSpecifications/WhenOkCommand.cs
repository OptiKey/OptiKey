using JuliusSweetland.OptiKey.Properties;
using NUnit.Framework;
using System.Windows;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    public abstract class WhenOkCommand : ManagementViewModelTestBase
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
            ManagementViewModel.OkCommand.Execute(Window);
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartRequired : WhenOkCommand
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.Debug = true;
        }

        [RequiresSTA]
        [Test]
        public void ThenWindowShouldNotBeClosed()
        {
            Assert.IsFalse(IsWindowClosed);
        }
    }

    [TestFixture]
    public class WhenOkCommandGivenRestartNotRequired : WhenOkCommand
    {
        [RequiresSTA]
        [Test]
        public void ThenWindowShouldBeClosed()
        {
            Assert.IsTrue(IsWindowClosed);
        }
    }
}
