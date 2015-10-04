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
            if (wordsViewModel.Language == Enums.Languages.FrenchFrance)
            {
                wordsViewModel.Language = Enums.Languages.EnglishUK;
            } else
            {
                wordsViewModel.Language = Enums.Languages.FrenchFrance;
            }

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
            wordsViewModel.Language = wordsViewModel.Language;

            wordsViewModel.ApplyChanges();

            //Assert
            mockDictionaryService.Verify(t => t.LoadDictionary(), Times.Exactly(0));
        }
    }
}
