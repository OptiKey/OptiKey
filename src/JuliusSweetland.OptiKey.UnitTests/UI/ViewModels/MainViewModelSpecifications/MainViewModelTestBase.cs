// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels;
using Moq;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    public abstract class MainViewModelTestBase : TestBase
    {
        protected MainViewModel MainViewModel { get; set; }
        protected Mock<IAudioService> AudioService { get; private set; }
        protected Mock<ICalibrationService> CalibrationService { get; private set; }
        protected Mock<ICapturingStateManager> CapturingStateManager { get; private set; }
        protected Mock<IDictionaryService> DictionaryService { get; private set; }
        protected Mock<IInputService> InputService { get; private set; }
        protected Mock<IKeyboardOutputService> KeyboardOutputService { get; private set; }
        protected Mock<IKeyStateService> KeyStateService { get; private set; }
        protected Mock<ILastMouseActionStateManager> LastMouseActionStateManager { get; private set; }
        protected Mock<IWindowManipulationService> MainWindowManipulationService { get; private set; }
        protected Mock<IMouseOutputService> MouseOutputService { get; private set; }
        protected Mock<ISuggestionStateService> SuggestionService { get; private set; }
        protected List<INotifyErrors> ErrorNotifyingServices { get; private set; }
        protected bool IsKeySelectionEventHandlerCalled { get; private set; }
        protected bool IsPointSelectionEventHandlerCalled { get; private set; }

        protected virtual bool ShouldConstruct { get { return true; } }

        protected override void Arrange()
        {
            AudioService = new Mock<IAudioService>();
            CalibrationService = new Mock<ICalibrationService>();
            CapturingStateManager = new Mock<ICapturingStateManager>();
            DictionaryService = new Mock<IDictionaryService>();
            InputService = new Mock<IInputService>();
            KeyboardOutputService = new Mock<IKeyboardOutputService>();

            KeyStateService = new Mock<IKeyStateService>();
            KeyStateService.Setup(s => s.KeyDownStates).Returns(new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>());
            KeyStateService.Setup(s => s.KeySelectionProgress).Returns(new NotifyingConcurrentDictionary<KeyValue, double>());

            LastMouseActionStateManager = new Mock<ILastMouseActionStateManager>();
            MainWindowManipulationService = new Mock<IWindowManipulationService>();
            MouseOutputService = new Mock<IMouseOutputService>();
            SuggestionService = new Mock<ISuggestionStateService>();
            ErrorNotifyingServices = new List<INotifyErrors>();

            if (ShouldConstruct)
            {
                MainViewModel = new MainViewModel(AudioService.Object, CalibrationService.Object, DictionaryService.Object,
                    KeyStateService.Object, SuggestionService.Object, CapturingStateManager.Object, LastMouseActionStateManager.Object,
                    InputService.Object, KeyboardOutputService.Object, MouseOutputService.Object, MainWindowManipulationService.Object,
                    ErrorNotifyingServices);

                MainViewModel.KeySelection += (s, e) => IsKeySelectionEventHandlerCalled = true;
                MainViewModel.PointSelection += (s, e) => IsPointSelectionEventHandlerCalled = true;
            }
        }

        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }
    }
}
