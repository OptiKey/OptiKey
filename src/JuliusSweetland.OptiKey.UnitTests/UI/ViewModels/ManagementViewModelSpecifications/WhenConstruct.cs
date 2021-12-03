// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.ViewModels;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.ManagementViewModelSpecifications
{
    [TestFixture]
    public class WhenConstruct : ManagementViewModelTestBase
    {
        protected override bool ShouldConstruct
        {
            get
            {
                return false;
            }
        }

        protected override void Act()
        {
            ManagementViewModel = new ManagementViewModel(AudioService.Object, DictionaryService.Object, WindowManipulationService.Object);
        }

        [Test]
        public void ThenChildViewModelShouldBeConstructed()
        {
            Assert.IsNotNull(ManagementViewModel.DictionaryViewModel);
            Assert.IsNotNull(ManagementViewModel.FeaturesViewModel);
            Assert.IsNotNull(ManagementViewModel.GesturesViewModel);
            Assert.IsNotNull(ManagementViewModel.PointingAndSelectingViewModel);
            Assert.IsNotNull(ManagementViewModel.SoundsViewModel);
            Assert.IsNotNull(ManagementViewModel.VisualsViewModel);
            Assert.IsNotNull(ManagementViewModel.WordsViewModel);
        }

        [Test]
        public void ThenInteractionRequestsAndCommandsShouldBeConstructed()
        {
            Assert.Multiple(() => 
            {
                Assert.That(ManagementViewModel.ConfirmationRequest, Is.Not.Null); 
                Assert.That(ManagementViewModel.OkCommand, Is.Not.Null);
                Assert.That(ManagementViewModel.CancelCommand, Is.Not.Null);
            });
        }
    }
}
