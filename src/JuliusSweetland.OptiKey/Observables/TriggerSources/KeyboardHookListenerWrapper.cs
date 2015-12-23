using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public sealed class KeyboardHookListenerWrapper : IKeyboardHookListener
    {
        private readonly KeyboardHookListener underlying;
        private readonly IObservable<System.Windows.Forms.Keys> keyDowns;
        private readonly IObservable<System.Windows.Forms.Keys> keyUps;

        public KeyboardHookListenerWrapper()
        {
            this.underlying = new KeyboardHookListener(new GlobalHooker())
            {
                Enabled = true
            };
            keyDowns = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                handler => new KeyEventHandler(handler),
                h => underlying.KeyDown += h,
                h => underlying.KeyDown -= h)
                .Select(e => e.EventArgs.KeyCode);

            keyUps = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                handler => new KeyEventHandler(handler),
                h => underlying.KeyUp += h,
                h => underlying.KeyUp -= h)
                .Select(e => e.EventArgs.KeyCode);
        }

        public IObservable<KeyDirection> KeyMovements(System.Windows.Forms.Keys triggerKey)
        {
            var downs = keyDowns.Where(keyCode => keyCode == triggerKey).Select(_ => KeyDirection.KeyDown);
            var ups = keyUps.Where(keyCode => keyCode == triggerKey).Select(_ => KeyDirection.KeyUp);

            return downs.Merge(ups)
                .DistinctUntilChanged()
                .SkipWhile(keyDirection=> !keyDirection.IsKeyDown);//Ensure the first value we hit is a true, i.e. a key down
        } 
    }
}