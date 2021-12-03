// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IMouseOutputService
    {
        Point GetCursorPosition();
        void LeftButtonClick();
        void LeftButtonDoubleClick();
        void LeftButtonDown();
        void LeftButtonUp();
        void MiddleButtonClick();
        void MiddleButtonDown();
        void MiddleButtonUp();
        void MoveAndLeftClick(Point point, bool sleep = false);
        void MoveAndLeftDoubleClick(Point point, bool sleep = false);
        void MoveAndMiddleClick(Point point, bool sleep = false);
        void MoveAndRightClick(Point point, bool sleep = false);
        void MoveAndScrollWheelUp(Point point, int clicks, bool sleep = false);
        void MoveAndScrollWheelDown(Point point, int clicks, bool sleep = false);
        void MoveAndScrollWheelLeft(Point point, int clicks, bool sleep = false);
        void MoveAndScrollWheelRight(Point point, int clicks, bool sleep = false);
        void MoveTo(Point point);
        void RightButtonClick();
        void RightButtonDown();
        void RightButtonUp();
        void ScrollWheelAbsolute(int dx, int dy);
    }
}
