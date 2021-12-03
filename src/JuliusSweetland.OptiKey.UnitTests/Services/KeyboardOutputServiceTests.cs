// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using Moq;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Services
{
    [TestFixture]
    public class KeyboardOutputServiceTests
    {
        private static readonly string dzSingleCodePoint = "\u01F3";
        private static readonly string dzMultiCodePoint = "\u0064\u007A";

        private static readonly string singleCodePointString = "12" + dzSingleCodePoint + "34";
        private static readonly string multiCodePointString = "12" + dzMultiCodePoint + "34";

        [Test]
        public void SuggestionSingleCodePointToMultiCodePoint()
        {
            //Arrange
            var mockKeyStateService = new Mock<IKeyStateService>();
            mockKeyStateService.Setup(x => x.KeyDownStates).Returns(new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>());

            var mockSuggestionService = new Mock<ISuggestionStateService>();
            mockSuggestionService.Setup(x => x.Suggestions).Returns(new List<string> { singleCodePointString, multiCodePointString });

            var mockPublishService = new Mock<IPublishService>();

            var mockDictionaryService = new Mock<IDictionaryService>();

            var keyboardOutputService = new KeyboardOutputService(mockKeyStateService.Object, mockSuggestionService.Object, mockPublishService.Object, mockDictionaryService.Object, x => { });

            //Act
            keyboardOutputService.ProcessSingleKeyText("1");
            keyboardOutputService.ProcessFunctionKey(FunctionKeys.Suggestion1);
            keyboardOutputService.ProcessFunctionKey(FunctionKeys.Suggestion2);

            //Assert
            Assert.That(keyboardOutputService.Text, Is.EqualTo(multiCodePointString));
        }

        [Test]
        public void SuggestionMultiCodePointToSingleCodePoint()
        {
            //Arrange
            var mockKeyStateService = new Mock<IKeyStateService>();
            mockKeyStateService.Setup(x => x.KeyDownStates).Returns(new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>());

            var mockSuggestionService = new Mock<ISuggestionStateService>();
            mockSuggestionService.Setup(x => x.Suggestions).Returns(new List<string> { singleCodePointString, multiCodePointString });

            var mockPublishService = new Mock<IPublishService>();

            var mockDictionaryService = new Mock<IDictionaryService>();

            var keyboardOutputService = new KeyboardOutputService(mockKeyStateService.Object, mockSuggestionService.Object, mockPublishService.Object, mockDictionaryService.Object, x => { });

            //Act
            keyboardOutputService.ProcessSingleKeyText("1");
            keyboardOutputService.ProcessFunctionKey(FunctionKeys.Suggestion2);
            keyboardOutputService.ProcessFunctionKey(FunctionKeys.Suggestion1);

            //Assert
            Assert.That(keyboardOutputService.Text, Is.EqualTo(singleCodePointString));
        }
    }
}