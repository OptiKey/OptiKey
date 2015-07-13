using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IOutputService
    {
        string Text { get; }
        bool ScratchpadIsEnabled { get; set; }
        
        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);

        void LeftMouseButtonDown(Point point);
        void LeftMouseButtonUp(Point point);
        void LeftMouseButtonClick(Point point);
        void LeftMouseButtonDoubleClick(Point point);
        void MiddleMouseButtonClick(Point point);
        void MoveMouseTo(Point point);
        void RightMouseButtonClick(Point point);
        void ScrollMouseWheelUp(int clicks, Point point);
        void ScrollMouseWheelUpAndLeft(int clicks, Point point);
        void ScrollMouseWheelUpAndRight(int clicks, Point point);
        void ScrollMouseWheelDown(int clicks, Point point);
        void ScrollMouseWheelDownAndLeft(int clicks, Point point);
        void ScrollMouseWheelDownAndRight(int clicks, Point point);
        void ScrollMouseWheelLeft(int clicks, Point point);
        void ScrollMouseWheelRight(int clicks, Point point);
    }
}
