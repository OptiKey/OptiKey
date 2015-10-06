using JuliusSweetland.OptiKey.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.UnitTests.Services
{
    [TestFixture]
    public class KeyboardOutputServiceTests
    {
        private static readonly string dzSingleCodePoint = "\u01F3";
        private static readonly string dzMultiCodePoint = "\u0064\u007A";

        private static readonly string singleCodePointString = "12" + dzSingleCodePoint + "34";
        private static readonly string multiCodePointString = "12" + dzMultiCodePoint +"34";

        [Test]
        public void SuggestionSingleCodePointToMultiCodePoint()
        {
            //Arrange
            var mockKeyStateService = new Mock<IKeyStateService>();
            mockKeyStateService.Setup(x => x.KeyDownStates).Returns(new Models.NotifyingConcurrentDictionary<Models.KeyValue, Enums.KeyDownStates>());

            var mockSuggestionService = new Mock<ISuggestionStateService>();
            mockSuggestionService.Setup(x => x.Suggestions).Returns(new List<string> { singleCodePointString, multiCodePointString });

            var mockPublishService = new Mock<IPublishService>();

            var mockDictionaryService = new Mock<IDictionaryService>();

            var keyboardOutputService = new KeyboardOutputService(mockKeyStateService.Object, mockSuggestionService.Object, mockPublishService.Object, mockDictionaryService.Object, x => { });

            //Act
            keyboardOutputService.ProcessSingleKeyText("1");
            keyboardOutputService.ProcessFunctionKey(Enums.FunctionKeys.Suggestion1);
            keyboardOutputService.ProcessFunctionKey(Enums.FunctionKeys.Suggestion2);

            //Assert
            Assert.AreEqual(multiCodePointString, keyboardOutputService.Text);
        }

        [Test]
        public void SuggestionMultiCodePointToSingleCodePoint()
        {
            //Arrange
            var mockKeyStateService = new Mock<IKeyStateService>();
            mockKeyStateService.Setup(x => x.KeyDownStates).Returns(new Models.NotifyingConcurrentDictionary<Models.KeyValue, Enums.KeyDownStates>());

            var mockSuggestionService = new Mock<ISuggestionStateService>();
            mockSuggestionService.Setup(x => x.Suggestions).Returns(new List<string> { singleCodePointString, multiCodePointString });

            var mockPublishService = new Mock<IPublishService>();

            var mockDictionaryService = new Mock<IDictionaryService>();

            var keyboardOutputService = new KeyboardOutputService(mockKeyStateService.Object, mockSuggestionService.Object, mockPublishService.Object, mockDictionaryService.Object, x => { });

            //Act
            keyboardOutputService.ProcessSingleKeyText("1");
            keyboardOutputService.ProcessFunctionKey(Enums.FunctionKeys.Suggestion2);
            keyboardOutputService.ProcessFunctionKey(Enums.FunctionKeys.Suggestion1);

            //Assert
            Assert.AreEqual(singleCodePointString, keyboardOutputService.Text);
        }
    }
}
