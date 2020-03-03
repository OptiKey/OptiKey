using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Services.Translation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace JuliusSweetland.OptiKey.UnitTests.Services.Translation
{
    [TestClass]
    public class TranslationServiceTests
    {
        /*
         * Tests the correct request is sent and mocked response is converted properly
         */
        [TestMethod]
        public async Task TestTranslateAsync()
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{'text':['Hallo'],'lang':'de','code':'200'}"),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var subjectUnderTest = new TranslationService(httpClient);
            TranslationService.TranslationTargetLanguage = "de";
                
            // ACT
            TranslationService.Response result = await subjectUnderTest.translate("hello");

            // ASSERT
            Assert.IsNotNull(result);
            // Mocked Http response should be converted into the response struct within TranslationService
            Assert.AreEqual("", result.exceptionMessage);
            Assert.AreEqual("Success", result.status);
            Assert.AreEqual("Hallo", result.translatedText);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                    "lang=" + TranslationService.TranslationTargetLanguage +
                    "&key=" + TranslationAPI.Default.ApiKey +
                    "&text=" + "hello");

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
