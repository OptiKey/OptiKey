// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.Management
{
    [TestFixture]
    public class WordsViewModelTests
    {
        [Test]
        public void ReloadDictionaryWhenLanguageChanged(){
            //Arrange
            var mockDictionaryService = new Mock<IDictionaryService>();

            var wordsViewModel = new WordsViewModel(mockDictionaryService.Object);

            //Act
            wordsViewModel.KeyboardAndDictionaryLanguage = wordsViewModel.KeyboardAndDictionaryLanguage == Enums.Languages.FrenchFrance 
                ? Enums.Languages.EnglishUK 
                : Enums.Languages.FrenchFrance;

            wordsViewModel.ApplyChanges();

            //Assert
            mockDictionaryService.Verify(t => t.LoadDictionary(), Times.AtLeast(1));
        }

        [Test]
        public void DoNotReloadDictionaryWhenLanguageIsTheSame(){
            //Arrange
            var mockDictionaryService = new Mock<IDictionaryService>();

            var wordsViewModel = new WordsViewModel(mockDictionaryService.Object);

            //Act
            wordsViewModel.KeyboardAndDictionaryLanguage = wordsViewModel.KeyboardAndDictionaryLanguage;

            wordsViewModel.ApplyChanges();

            //Assert
            mockDictionaryService.Verify(t => t.LoadDictionary(), Times.Exactly(0));
        }
    }
}
