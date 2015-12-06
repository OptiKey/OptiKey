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
        protected Mock<IConfigurableCommandService> ConfigurableCommandService { get; private set; }

        protected virtual bool ShouldConstruct { get { return true; } }

        protected override void Arrange()
        {
            AudioService = new Mock<IAudioService>();
            DictionaryService = new Mock<IDictionaryService>();
            ConfigurableCommandService = new Mock<IConfigurableCommandService>();

            if (ShouldConstruct)
            {
                ManagementViewModel = new ManagementViewModel(AudioService.Object, DictionaryService.Object, ConfigurableCommandService.Object);
            }
        }
    }
}
