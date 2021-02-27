// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
            Assert.Multiple(() => 
            { 
                Assert.That(ManagementViewModel.DictionaryViewModel, Is.Not.Null); 
                Assert.That(ManagementViewModel.OtherViewModel, Is.Not.Null); 
                Assert.That(ManagementViewModel.PointingAndSelectingViewModel, Is.Not.Null); 
                Assert.That(ManagementViewModel.SoundsViewModel, Is.Not.Null); 
                Assert.That(ManagementViewModel.VisualsViewModel, Is.Not.Null); 
                Assert.That(ManagementViewModel.WordsViewModel, Is.Not.Null); 
            });
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
