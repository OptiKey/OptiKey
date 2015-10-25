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

        [Test]
        public void ThenSimulateKeyStrokesShouldBeSetToTheConfiguredKeyboard()
        {
            KeyStateService.VerifySet(s => s.SimulateKeyStrokes = MainViewModel.Keyboard.SimulateKeyStrokes, Times.Once());
        }

        [Test]
        public void ThenMultiKeySelectionSupportedShouldBeSetToTheConfiguredKeyboard()
        {
            InputService.VerifySet(s => s.MultiKeySelectionSupported = MainViewModel.Keyboard.MultiKeySelectionSupported, Times.Once());
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
        public void ThenWindowShouldBeMaximised()
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
        public void ThenWindowShouldBeMaximised()
        {
            MainWindowManipulationService.Verify(s => s.Maximise(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardConversationNumericAndSymbols : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.ConversationNumericAndSymbols;
        }

        [Test]
        public void ThenKeyboardShouldBeConversationNumericAndSymbolsWithA_BackAction()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.ConversationNumericAndSymbols), MainViewModel.Keyboard.GetType());

            var conversationAlpha = MainViewModel.Keyboard as OptiKey.UI.ViewModels.Keyboards.ConversationNumericAndSymbols;
            Assert.AreEqual(typeof(Action), conversationAlpha.BackAction.GetType());
        }

        [Test]
        public void ThenWindowShouldBeMaximised()
        {
            MainWindowManipulationService.Verify(s => s.Maximise(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardCurrencies1 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Currencies1;
        }

        [Test]
        public void ThenKeyboardShouldBeCurrencies1()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Currencies1), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardCurrencies2 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Currencies2;
        }

        [Test]
        public void ThenKeyboardShouldBeCurrencies2()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Currencies2), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardDiacritics1 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Diacritics1;
        }

        [Test]
        public void ThenKeyboardShouldBeDiacritics1()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Diacritics1), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardDiacritics2 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Diacritics2;
        }

        [Test]
        public void ThenKeyboardShouldBeDiacritics1()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Diacritics2), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardDiacritics3 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Diacritics3;
        }

        [Test]
        public void ThenKeyboardShouldBeDiacritics1()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Diacritics3), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardMenu : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Menu;
        }

        [Test]
        public void ThenKeyboardShouldBeMenu()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Menu), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardMinimised : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Minimised;
        }

        [Test]
        public void ThenKeyboardShouldBeMinimisedWithA_BackAction()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Minimised), MainViewModel.Keyboard.GetType());

            var alpha = MainViewModel.Keyboard as OptiKey.UI.ViewModels.Keyboards.Minimised;
            Assert.AreEqual(typeof(Action), alpha.BackAction.GetType());
        }

        [Test]
        public void ThenWindowShouldBeMinimised()
        {
            MainWindowManipulationService.Verify(s => s.Minimise(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardMouseAndMouseKeyboardDockSizeIsFull : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Mouse;
            Settings.Default.MouseKeyboardDockSize = DockSizes.Full;
        }

        [Test]
        public void ThenKeyboardShouldBeMouseWithA_BackAction()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Mouse), MainViewModel.Keyboard.GetType());

            var alpha = MainViewModel.Keyboard as OptiKey.UI.ViewModels.Keyboards.Mouse;
            Assert.AreEqual(typeof(Action), alpha.BackAction.GetType());
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
    public class WhenConstructGivenStartupKeyboardMouseAndMouseKeyboardDockSizeIsCollapsed : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.Mouse;
            Settings.Default.MouseKeyboardDockSize = DockSizes.Collapsed;
        }

        [Test]
        public void ThenKeyboardShouldBeMouseWithA_BackAction()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.Mouse), MainViewModel.Keyboard.GetType());

            var alpha = MainViewModel.Keyboard as OptiKey.UI.ViewModels.Keyboards.Mouse;
            Assert.AreEqual(typeof(Action), alpha.BackAction.GetType());
        }

        [Test]
        public void ThenWindowShouldBeRestored()
        {
            MainWindowManipulationService.Verify(s => s.Restore(), Times.Once());
        }

        [Test]
        public void ThenDockShouldBeCollapsed()
        {
            MainWindowManipulationService.Verify(s => s.ResizeDockToCollapsed(), Times.Once());
        }
    }

    [TestFixture]
    public class WhenConstructGivenStartupKeyboardNumericAndSymbols1 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.NumericAndSymbols1;
        }

        [Test]
        public void ThenKeyboardShouldBeNumericAndSymbols1()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.NumericAndSymbols1), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardNumericAndSymbols2 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.NumericAndSymbols2;
        }

        [Test]
        public void ThenKeyboardShouldBeNumericAndSymbols2()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.NumericAndSymbols2), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardNumericAndSymbols3 : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.NumericAndSymbols3;
        }

        [Test]
        public void ThenKeyboardShouldBeNumericAndSymbols3()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.NumericAndSymbols3), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardPhysicalKeys : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.PhysicalKeys;
        }

        [Test]
        public void ThenKeyboardShouldBePhysicalKeys()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.PhysicalKeys), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenStartupKeyboardSizeAndPosition : WhenConstruct
    {
        protected override void Arrange()
        {
            base.Arrange();
            Settings.Default.ConversationOnlyMode = false;
            Settings.Default.StartupKeyboard = Keyboards.SizeAndPosition;
        }

        [Test]
        public void ThenKeyboardShouldBeSizeAndPosition()
        {
            Assert.IsNotNull(MainViewModel.Keyboard);
            Assert.AreEqual(typeof(OptiKey.UI.ViewModels.Keyboards.SizeAndPosition), MainViewModel.Keyboard.GetType());
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
    public class WhenConstructGivenKeyboardSupportsCollapsedDock : WhenConstruct
    {
        [Test]
        public void ThenDockShouldNotBeResizedToFull()
        {
            MainWindowManipulationService.Verify(s => s.ResizeDockToFull(), Times.AtMostOnce(),
                "Should have been called at most once by InitialiseKeyboard");
        }
    }
}
