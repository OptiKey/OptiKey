using System.Windows;
using System.Windows.Forms;
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
        void LeftMouseButtonDown();
        void LeftMouseButtonUp();
        void LeftMouseButtonClick();
        void LeftMouseButtonDoubleClick();
        void RightMouseButtonClick();
        void ScrollMouseWheelUp(int clicks);
        void ScrollMouseWheelUpAndLeft(int clicks);
        void ScrollMouseWheelUpAndRight(int clicks);
        void ScrollMouseWheelDown(int clicks);
        void ScrollMouseWheelDownAndLeft(int clicks);
        void ScrollMouseWheelDownAndRight(int clicks);
        void ScrollMouseWheelLeft(int clicks);
        void ScrollMouseWheelRight(int clicks);
    }
}
