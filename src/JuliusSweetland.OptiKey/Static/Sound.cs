using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Native;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Sound
    {
        public static int GetSoundLength(string fileName)
        {
            int length = 0;
            if (fileName.StartsWith("http://localhost:59125/process?"))
            {
                string timeURL = fileName.Replace("OUTPUT_TYPE=AUDIO", "OUTPUT_TYPE=REALISED_DURATIONS");
                List<string> realised_durations = new List<string>();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(timeURL);

                // Set some reasonable limits on resources used by this request
                request.MaximumAutomaticRedirections = 4;
                request.MaximumResponseHeadersLength = 4;
                // Set credentials to use for this request.
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string responseText = readStream.ReadToEnd();

                realised_durations = responseText.Split('\n').ToList();

                // retrieve the time of the last syllable
                length = (int)(1000 * Convert.ToSingle(realised_durations.ElementAt(realised_durations.Count() - 2).Split(' ').ToList().ElementAt(0)));

                //Log.InfoFormat("MaryTTS speech ends in {0} ms", delaytime);
            }
            else
            {
                StringBuilder lengthBuf = new StringBuilder(32);

                PInvoke.mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
                PInvoke.mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
                PInvoke.mciSendString("close wave", null, 0, IntPtr.Zero);

                int.TryParse(lengthBuf.ToString(), out length);
            }

            return length;
        }
    }
}
