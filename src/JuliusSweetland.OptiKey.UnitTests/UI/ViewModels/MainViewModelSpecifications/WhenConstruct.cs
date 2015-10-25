using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels;
using Moq;
using NUnit.Framework;
using System;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    public abstract class WhenConstruct : MainViewModelTestBase
    {
        protected override void Act()
        {
            MainViewModel = new MainViewModel(AudioService.Object, CalibrationService.Object, DictionaryService.Object, 
                KeyStateService.Object, SuggestionService.Object, CapturingStateManager.Object, LastMouseActionStateManager.Object, 
                InputService.Object, KeyboardOutputService.Object, MouseOutputService.Object, MainWindowManipulationService.Object, 
                ErrorNotifyingServices);
        }
    }

    [TestFixture]
    public class WhenConstructGivenConversationOnlyMode : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = true;
        }

        [Test]
        public void ThenWindowShouldBeMaximized()
        {
            MainWindowManipulationService.Verify(s => s.Maximise(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardAlpha : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Alpha;
        }

        [Test]
        public void ThenKeyboardShouldBeAlpha()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Alpha), MainViewModel.Keyboard.GetType());
        }

        [Test]
        public void ThenWindowShouldBeRestored()
        {
            MainWindowManipulationService.Verify(s => s.Restore(), Times.Once());
        }

        [Test]
        public void ThenDockShouldBeResizedToFull()
        {
            MainWindowManipulationService.Verify(s => s.ResizeDockToFull(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardConversationAlpha : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.ConversationAlpha;
        }

        [Test]
        public void ThenKeyboardShouldBeConversationAlphaWithA_BackAction()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.ConversationAlpha), MainViewModel.Keyboard.GetType());

            var conversationAlpha = MainViewModel.Keyboard as OptiKey.UI.ViewModels.Keyboards.ConversationAlpha;
            Assert.AreEqual(typeof(Action), conversationAlpha.BackAction.GetType());
        }

        [Test]
        public void ThenWindowShouldBeMaximized()
        {
            MainWindowManipulationService.Verify(s => s.Maximise(), Times.Once());
        }
    }
}
