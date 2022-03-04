// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using WindowsInput.Native;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IPublishService : INotifyErrors
    {
        void KeyDown(VirtualKeyCode virtualKeyCode);
        void KeyUp(VirtualKeyCode virtualKeyCode);
        void KeyDownUp(VirtualKeyCode virtualKeyCode);
        void LeftMouseButtonClick();
        void LeftMouseButtonDoubleClick();
        void LeftMouseButtonDown();
        void LeftMouseButtonUp();
        void MiddleMouseButtonClick();
        void MiddleMouseButtonDown();
        void MiddleMouseButtonUp();
        void MouseMouseToPoint(Point point);
        void ReleaseAllDownKeys();
        void RightMouseButtonClick();
        void RightMouseButtonDown();
        void RightMouseButtonUp();
        void ScrollMouseWheelUp(int clicks);
        void ScrollMouseWheelDown(int clicks);
        void ScrollMouseWheelLeft(int clicks);
        void ScrollMouseWheelRight(int clicks);
        void ScrollMouseWheelAbsoluteHorizontal(int amount);
        void ScrollMouseWheelAbsoluteVertical(int amount);
        void TypeText(string text);
    }
}
