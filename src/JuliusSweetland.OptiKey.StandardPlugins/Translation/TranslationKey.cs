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

    /*
     * Azerbaijan	az	Malayalam	ml
Albanian	sq	Maltese	mt
Amharic	am	Macedonian	mk
English	en	Maori	mi
Arabic	ar	Marathi	mr
Armenian	hy	Mari	mhr
Afrikaans	af	Mongolian	mn
Basque	eu	German	de
Bashkir	ba	Nepali	ne
Belarusian	be	Norwegian	no
Bengali	bn	Punjabi	pa
Burmese	my	Papiamento	pap
Bulgarian	bg	Persian	fa
Bosnian	bs	Polish	pl
Welsh	cy	Portuguese	pt
Hungarian	hu	Romanian	ro
Vietnamese	vi	Russian	ru
Haitian (Creole)	ht	Cebuano	ceb
Galician	gl	Serbian	sr
Dutch	nl	Sinhala	si
Hill Mari	mrj	Slovakian	sk
Greek	el	Slovenian	sl
Georgian	ka	Swahili	sw
Gujarati	gu	Sundanese	su
Danish	da	Tajik	tg
Hebrew	he	Thai	th
Yiddish	yi	Tagalog	tl
Indonesian	id	Tamil	ta
Irish	ga	Tatar	tt
Italian	it	Telugu	te
Icelandic	is	Turkish	tr
Spanish	es	Udmurt	udm
Kazakh	kk	Uzbek	uz
Kannada	kn	Ukrainian	uk
Catalan	ca	Urdu	ur
Kyrgyz	ky	Finnish	fi
Chinese	zh	French	fr
Korean	ko	Hindi	hi
Xhosa	xh	Croatian	hr
Khmer	km	Czech	cs
Laotian	lo	Swedish	sv
Latin	la	Scottish	gd
Latvian	lv	Estonian	et
Lithuanian	lt	Esperanto	eo
Luxembourgish	lb	Javanese	jv
Malagasy	mg	Japanese	ja
Malay	ms
     */
    enum TargetTranslationLanguageCodes {
        //ABC(AZ),
        //SQ
            

    }
}
