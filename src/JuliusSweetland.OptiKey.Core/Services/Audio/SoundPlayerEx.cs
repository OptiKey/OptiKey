// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services.Audio
{
    public class SoundPlayerEx : SoundPlayer
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool Finished { get; private set; }
        
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken ct;
        private bool playingAsync;

        public event EventHandler SoundFinished;

        public SoundPlayerEx()
        {
            ct = tokenSource.Token;
        }

        public async Task PlayAsync(Action<Exception> onError)
        {
            Log.DebugFormat("Attempting to play sound asynchronously.");
            Finished = false;
            playingAsync = true;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        double lenMs = Sound.GetSoundLength(SoundLocation);
                        DateTime stopAt = DateTime.Now.AddMilliseconds(lenMs);
                        Log.DebugFormat("Sound ends at {0}.", stopAt);
                        this.Play();
                        while (DateTime.Now < stopAt)
                        {
                            ct.ThrowIfCancellationRequested();
                            //The delay helps reduce processor usage while "spinning"
                            Task.Delay(10).Wait();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        base.Stop();
                        Log.DebugFormat("Sound manually stoped. Generating a new CancellationTokenSource");
                        // Create new CancellationTokenSource
                        tokenSource.Dispose();
                        tokenSource = new CancellationTokenSource();
                        ct = tokenSource.Token;
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                Log.Error("Error when calling SoundPlayerEx.PlayAsync (MaryTTS).", ex);
                onError(ex);
            }
            finally
            {
                OnSoundFinished();
            }
        }

        public new void Stop()
        {
            if (playingAsync)
                tokenSource.Cancel();
            else
                base.Stop();   //To stop the SoundPlayer Wave file
        }

        protected virtual void OnSoundFinished()
        {
            Log.DebugFormat("Sound Finished.");
            Finished = true;
            playingAsync = false;

            EventHandler handler = SoundFinished;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}