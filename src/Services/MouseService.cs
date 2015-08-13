using System.Windows;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class MouseService : IMouseService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPublishService publishService;

        #endregion

        #region Ctor

        public MouseService(IPublishService publishService)
        {
            this.publishService = publishService;
        }

        #endregion

        #region Methods - IMouseService

        public void LeftButtonClick()
        {
            Log.Debug("Generating a left click.");
            publishService.LeftMouseButtonClick();
        }

        public void LeftButtonDoubleClick()
        {
            Log.Debug("Generating a left double click.");
            publishService.LeftMouseButtonDoubleClick();
        }

        public void LeftButtonDown()
        {
            Log.Debug("Pressing down left button.");
            publishService.LeftMouseButtonDown();
        }

        public void LeftButtonUp()
        {
            Log.Debug("Releasing left button.");
            publishService.LeftMouseButtonUp();
        }

        public void MiddleButtonClick()
        {
            Log.DebugFormat("Generating a middle click.");
            publishService.MiddleMouseButtonClick();
        }

        public void MiddleButtonDown()
        {
            Log.Debug("Pressing down middle button.");
            publishService.MiddleMouseButtonDown();
        }

        public void MiddleButtonUp()
        {
            Log.Debug("Releasing middle button.");
            publishService.MiddleMouseButtonUp();
        }

        public void MoveTo(Point point)
        {
            Log.DebugFormat("Moving cursor to point '{0}'", point);
            publishService.MouseMouseToPoint(point);
        }

        public void RightButtonClick()
        {
            Log.DebugFormat("Generating a right click.");
            publishService.RightMouseButtonClick();
        }

        public void RightButtonDown()
        {
            Log.Debug("Pressing down right button.");
            publishService.RightMouseButtonDown();
        }

        public void RightButtonUp()
        {
            Log.Debug("Releasing right button.");
            publishService.RightMouseButtonUp();
        }

        public void ScrollWheelUp(int clicks)
        {
            Log.DebugFormat("Generating a vertical mouse scroll of {0} clicks up.", clicks);
            publishService.ScrollMouseWheelUp(clicks);
        }

        public void ScrollWheelDown(int clicks)
        {
            Log.DebugFormat("Generating a vertical mouse scroll of {0} clicks down.", clicks);
            publishService.ScrollMouseWheelDown(clicks);
        }

        public void ScrollWheelLeft(int clicks)
        {
            Log.DebugFormat("Generating a horizontal mouse scroll of {0} clicks left.", clicks);
            publishService.ScrollMouseWheelLeft(clicks);
        }

        public void ScrollWheelRight(int clicks)
        {
            Log.DebugFormat("Generating a horizontal mouse scroll of {0} clicks right.", clicks);
            publishService.ScrollMouseWheelRight(clicks);
        }

        #endregion
    }
}
