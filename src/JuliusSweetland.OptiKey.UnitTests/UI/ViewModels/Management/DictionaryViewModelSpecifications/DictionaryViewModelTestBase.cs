// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Moq;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.Management.DictionaryViewModelSpecifications
{
    public abstract class DictionaryViewModelTestBase : TestBase
    {
        protected DictionaryViewModel DictionaryViewModel { get; set; }
        protected Mock<IDictionaryService> DictionaryService { get; private set; }

        protected virtual bool ShouldConstruct { get { return true; } }

        protected override void Arrange()
        {
            DictionaryService = new Mock<IDictionaryService>();

            if (ShouldConstruct)
            {
                DictionaryViewModel = new DictionaryViewModel(DictionaryService.Object);
            }
        }
    }
}
