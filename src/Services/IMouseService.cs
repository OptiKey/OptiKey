using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IMouseService
    {
        void LeftButtonClick();
        void LeftButtonDoubleClick();
        void LeftButtonDown();
        void LeftButtonUp();
        void MiddleButtonClick();
        void MiddleButtonDown();
        void MiddleButtonUp();
        void MoveTo(Point point);
        void RightButtonClick();
        void RightButtonDown();
        void RightButtonUp();
        void ScrollWheelUp(int clicks);
        void ScrollWheelDown(int clicks);
        void ScrollWheelLeft(int clicks);
        void ScrollWheelRight(int clicks);
    }
}
