// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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

        public TranslationService(HttpClient httpClient)
        {
            this.client = httpClient;
        }

        public struct Response
        {
            public Response(string status, string translatedText, string exceptionMessage)
            {
                Status = status;
                TranslatedText = translatedText;
                ExceptionMessage = exceptionMessage;
            }

            public string Status { get; }
            public string TranslatedText { get; }
            public string ExceptionMessage { get; }
        }
        
        /*
         * Returns the translation status, translated text and exception content
         */
        public async Task<Response> Translate(string text)
        {
            Response response;

            if (Settings.Default.EnableOverrideTranslationApiKey //Override key may be set, but should not be used unless enabled
                && !string.IsNullOrEmpty(Settings.Default.OverriddenTranslationApiKey)) 
            {
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
                    response = new Response("Success", obj.Text[0], "");
                }
                else
                {
                    response = new Response("Error", "", "Translation response was empty! is target language set?");
                }

                return response;
            }
            catch (HttpRequestException exception)
            {   
                response = new Response("Error", "", exception.Message);
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

        private class YandexResponse
        {
            public int Code { get; set; }
            public string Lang { get; set; }
            public List<string> Text { get; set; }
        }
    }
}
