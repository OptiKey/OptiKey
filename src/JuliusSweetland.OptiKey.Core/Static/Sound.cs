// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using JuliusSweetland.OptiKey.Extensions;
using log4net;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Sound
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static int GetSoundLength(string fileName)
        {
            int length;
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
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    Log.Error("Unable to use MaryTTS voice synthesiser.");
                    if (File.Exists(Settings.Default.MaryTTSLocation))
                    {
                        Log.Error("Trying to restart MaryTTS server.");
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Minimized, // cannot close it if set to hidden
                                CreateNoWindow = true,
                                FileName = Settings.Default.MaryTTSLocation
                            }
                        };
                        try
                        {
                            proc.Start();
                        }
                        catch
                        {
                            Log.ErrorFormat("Failed to restart MaryTTS server. Disabling MaryTTS and using System Voice '{0}' instead.",
                                Settings.Default.SpeechVoice);
                            Settings.Default.MaryTTSEnabled = false;
                        }

                        if (proc.StartTime <= DateTime.Now && !proc.HasExited)
                        {
                            Log.InfoFormat("Restarted MaryTTS server at {0}.", proc.StartTime);
                            proc.CloseOnApplicationExit(Log, "MaryTTS");
                        }
                        else
                        {
                            var errorMsg = string.Format(
                                "Failed to started MaryTTS (server not running). Disabling MaryTTS and using System Voice '{0}' instead.",
                                Settings.Default.SpeechVoice);

                            if (proc.HasExited)
                            {
                                errorMsg = string.Format(
                                "Failed to started MaryTTS (server was closed). Disabling MaryTTS and using System Voice '{0}' instead.",
                                Settings.Default.SpeechVoice);
                            }

                            Log.Error(errorMsg);
                            Settings.Default.MaryTTSEnabled = false;
                        }
                    }
                    else
                    {
                        Log.ErrorFormat("Failed to restart MaryTTS server. Disabling MaryTTS and using System Voice '{0}' instead.",
                            Settings.Default.SpeechVoice);
                        Settings.Default.MaryTTSEnabled = false;
                    }
                }

                if (response != null)
                {
                    // Get the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    string responseText = readStream.ReadToEnd();

                    realised_durations = responseText.Split('\n').ToList();

                    // retrieve the time of the last syllable
                    length = (int)(1000 * Convert.ToSingle(realised_durations.ElementAt(realised_durations.Count() - 2).Split(' ').ToList().ElementAt(0)));
                    return length;
                }
            }
            else
            {
                StringBuilder lengthBuf = new StringBuilder(32);

                PInvoke.mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
                PInvoke.mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
                PInvoke.mciSendString("close wave", null, 0, IntPtr.Zero);

                int.TryParse(lengthBuf.ToString(), out length);
                return length;
            }

            return 0; //This is an error condition
        }
    }
}
