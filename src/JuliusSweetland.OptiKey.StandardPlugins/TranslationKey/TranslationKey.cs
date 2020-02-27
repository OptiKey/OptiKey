using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class TranslationKey
    {
        HttpClient client = new HttpClient();

        public async Task<string> Translate(string text)
        {
            HttpResponseMessage response = await client.GetAsync(
                "https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                "lang=de&" +
                "key=trnsl.1.1.20200208T211513Z.c27ba519478a018a.0686d91c86a4321b32b41b0c30551aef47556314&" +
                "text=" + text);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseBody)){
                YandexResponse obj = JsonConvert.DeserializeObject<YandexResponse>(responseBody);
                return obj.text[0];
            }
            return "Error translating text.";
        }
    }

    class YandexResponse
    {
        public int code { get; set; }
        public string lang { get; set; }
        public List<string> text { get; set; }
    }
}
