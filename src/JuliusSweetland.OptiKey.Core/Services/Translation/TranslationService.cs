// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.CodeDom.Compiler;
using System.CodeDom;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services.Translation
{
    public class TranslationService
    {
        private HttpClient client;
        private string apiKeyToUse;
        public const string LICENSE_TEXT = "(Powered by Yandex.Translate)";

        public TranslationService(HttpClient httpClient)
        {
            this.client = httpClient;
        }

        public struct Response
        {
            public string status;
            public string translatedText;
            public string exceptionMessage;
        }
        
        /*
         * Returns the translation status, translated text and exception content
         */
        public async Task<Response> translate(string text)
        {
            Response response;
            response.status = ""; response.translatedText = ""; response.exceptionMessage = "";

            if (!string.IsNullOrEmpty(Settings.Default.OverriddenTranslationApiKey)) {
                this.apiKeyToUse = Settings.Default.OverriddenTranslationApiKey;
            }
            else
            {
                this.apiKeyToUse = TranslationAPI.Default.ApiKey;
            }

            try
            {
                string escapedText = ToLiteral(text);

                HttpResponseMessage httpResponseMessage = await client.GetAsync(
                    "https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                    "lang=" + Settings.Default.TranslationTargetLanguage +
                    "&key=" + this.apiKeyToUse +
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
