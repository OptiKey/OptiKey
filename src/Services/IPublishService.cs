using System.Windows;
using WindowsInput.Native;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IPublishService : INotifyErrors
    {
        void KeyDown(VirtualKeyCode virtualKeyCode);
        void KeyUp(VirtualKeyCode virtualKeyCode);
        void KeyDownUp(VirtualKeyCode virtualKeyCode);
        void ReleaseAllDownKeys();
        void TypeText(string text);
        void MouseMouseToPoint(Point point);
        void LeftButtonClick();
    }
}
