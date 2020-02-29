using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using JuliusSweetland.OptiKey.StandardPlugins.Properties;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class Translator
    {
        private HttpClient client;
        public static string TranslationTargetLanguage;
  
        public struct Response
        {
            public string status;
            public string translatedText;
            public string exceptionMessage;
        }

        public Translator()
        {
            this.client = new HttpClient();
        }

        /*
         * Returns the translation status, translated text and exception content
         * 
         * This class cant access the toast notification / logger, due to circular dependency it would create,
         * therefore the exception must be passed back to MainViewModel to raise as toast notification / logged.
         */
        public async Task<Response> Translate(string text)
        {
            Response response;
            response.status = ""; response.translatedText = ""; response.exceptionMessage = "";
                
            try
            {
                string escapedText = ToLiteral(text);

                HttpResponseMessage httpResponseMessage = await client.GetAsync(
                    "https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                    "lang=" + TranslationTargetLanguage +
                    "&key=trnsl.1.1.20200208T211513Z.c27ba519478a018a.0686d91c86a4321b32b41b0c30551aef47556314" +
                    "&text=" + text);

                httpResponseMessage.EnsureSuccessStatusCode();

                string httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(httpResponseBody))
                {
                    YandexResponse obj = JsonConvert.DeserializeObject<YandexResponse>(httpResponseBody);
                    response.translatedText = obj.text[0];
                    response.status = "Success";
                }
                else
                {
                    response.status = "Error";
                }

                return response;
            }
            catch (HttpRequestException exception)
            {
                response.status = "Error";
                response.exceptionMessage = exception.Message;
                return response;
            }
        }

        private string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
    class YandexResponse
    {
        public int code { get; set; }
        public string lang { get; set; }
        public List<string> text { get; set; }
    }
}
