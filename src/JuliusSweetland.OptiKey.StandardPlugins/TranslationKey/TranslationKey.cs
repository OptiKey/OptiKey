using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net.Http;
using System.IO;

namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class TranslationKey
    {
        HttpClient client = new HttpClient();

        public void TRANSLATE(string text)
        {
            sendRequest();
        }

        private async void sendRequest()
        {
            HttpResponseMessage response = await client.GetAsync(
                "https://translate.yandex.net/api/v1.5/tr.json/translate?" +
                "lang=de&" +
                "key=trnsl.1.1.20200208T211513Z.c27ba519478a018a.0686d91c86a4321b32b41b0c30551aef47556314&" +
                "text=Hello");
            response.EnsureSuccessStatusCode();

            //string responseCode = response.StatusCode.ToString();
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Print("res is: " + responseBody);

            Stream str = new FileStream(".", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(str);
            sw.WriteLine(responseBody);

            Process.Start("notepad.exe", ".");
        }
      
    }

}
