using System;
using WindowsInput.Native;

namespace JuliusSweetland.ETTA.Services
{
    public interface IPublishService
    {
        event EventHandler<Exception> Error;

        void ReleaseAllDownKeys();
        void PublishKeyDown(VirtualKeyCode virtualKeyCode);
        void PublishKeyUp(VirtualKeyCode virtualKeyCode);
        void PublishKeyPress(VirtualKeyCode virtualKeyCode);
        void PublishText(string text);
    }
}
