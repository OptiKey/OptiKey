// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using NUnit.Framework;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.Management.DictionaryViewModelSpecifications
{
    [TestFixture]
    public class WhenConstruct : DictionaryViewModelTestBase
    {
        protected override bool ShouldConstruct
        {
            get
            {
                return false;
            }
        }

        protected List<DictionaryEntry> DictionaryEntries { get; set; }

        protected override void Arrange()
        {
            base.Arrange();

            DictionaryEntries = new List<DictionaryEntry> { new DictionaryEntry("a"), new DictionaryEntry("b") };

            DictionaryService.Setup(r => r.GetAllEntries())
                .Returns(DictionaryEntries);
        }

        protected override void Act()
        {
            DictionaryViewModel = new DictionaryViewModel(DictionaryService.Object);
        }

        [Test]
        public void ThenCommandsShouldBeConstructed()
        {
            Assert.IsNotNull(DictionaryViewModel.AddCommand);
            Assert.IsNotNull(DictionaryViewModel.ToggleDeleteCommand);
            Assert.IsNotNull(DictionaryViewModel.LoadCommand);
        }

        [Test]
        public void ThenEntriesShouldBeLoaded()
        {
            DictionaryViewModel.Load();
            Assert.IsNotNull(DictionaryViewModel.Entries);
            Assert.AreEqual(DictionaryEntries.Count, DictionaryViewModel.Entries.Count);
        }
    }
}
