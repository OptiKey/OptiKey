// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels;
using Moq;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    public abstract class ManagementViewModelTestBase : TestBase
    {
        protected ManagementViewModel ManagementViewModel { get; set; }
        protected Mock<IAudioService> AudioService { get; private set; }
        protected Mock<IDictionaryService> DictionaryService { get; private set; }
        protected Mock<IWindowManipulationService> WindowManipulationService { get; private set; }

        protected virtual bool ShouldConstruct { get { return true; } }

        protected override void Arrange()
        {
            AudioService = new Mock<IAudioService>();
            DictionaryService = new Mock<IDictionaryService>();
            WindowManipulationService = new Mock<IWindowManipulationService>();

            if (ShouldConstruct)
            {
                ManagementViewModel = new ManagementViewModel(AudioService.Object, DictionaryService.Object, WindowManipulationService.Object);
            }
        }
    }
}
