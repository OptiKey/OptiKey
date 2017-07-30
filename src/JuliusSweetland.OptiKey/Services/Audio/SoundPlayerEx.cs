using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Static;

namespace JuliusSweetland.OptiKey.Services.Audio
{
    public class SoundPlayerEx : SoundPlayer
    {
        public bool Finished { get; private set; }

        private Task playTask;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken ct;
        private bool playingAsync;

        public event EventHandler SoundFinished;

        public SoundPlayerEx()
        {
            ct = tokenSource.Token;
        }

        public void PlayAsync()
        {
            Finished = false;
            playingAsync = true;
            Task.Run(() =>
            {
                try
                {
                    double lenMs = Sound.GetSoundLength(SoundLocation);
                    DateTime stopAt = DateTime.Now.AddMilliseconds(lenMs);
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
                }
                finally
                {
                    OnSoundFinished();
                }
            }, ct).ContinueWith(antecedent => OnSoundFinished(), TaskScheduler.FromCurrentSynchronizationContext());
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
            Finished = true;
            playingAsync = false;

            EventHandler handler = SoundFinished;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}