using System;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Extensions;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowStateService : BindableBase, IWindowStateService
    {
        private Window window;
        public Window Window
        {
            set
            {
                SetProperty(ref window, value);

                if (window != null)
                {
                    window.OnPropertyChanges<WindowState>(Window.WindowStateProperty)
                        .ObserveOnDispatcher()
                        .Subscribe(ws => WindowState = ws);

                    window.Dispatcher.Invoke(() => WindowState = window.WindowState); //WindowState has thread affinity with the UI thread
                }
            }
        }

        private WindowState? windowState;
        public WindowState? WindowState
        {
            get { return windowState; }
            set { SetProperty(ref windowState, value); }
        }
    }
}
