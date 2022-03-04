// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class MouseOutputService : IMouseOutputService
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPublishService publishService;

        private const int DEFAULT_SLEEP_MS = 10;

        #endregion

        #region Ctor

        public MouseOutputService(IPublishService publishService)
        {
            this.publishService = publishService;
        }

        #endregion

        #region Methods - IMouseOutputService

        public Point GetCursorPosition()
        {
            var point = new Point(Cursor.Position.X, Cursor.Position.Y);
            Log.DebugFormat("Getting current cursor point ({0},{1}).", point.X, point.Y);
            return point;
        }

        public void LeftButtonClick()
        {
            Log.Info("Generating a left click.");
            publishService.LeftMouseButtonClick();
        }

        public void LeftButtonDoubleClick()
        {
            Log.Info("Generating a left double click.");
            publishService.LeftMouseButtonDoubleClick();
        }

        public void LeftButtonDown()
        {
            Log.Info("Pressing down left button.");
            publishService.LeftMouseButtonDown();
        }

        public void LeftButtonUp()
        {
            Log.Info("Releasing left button.");
            publishService.LeftMouseButtonUp();
        }

        public void MiddleButtonClick()
        {
            Log.DebugFormat("Generating a middle click.");
            publishService.MiddleMouseButtonClick();
        }

        public void MiddleButtonDown()
        {
            Log.Info("Pressing down middle button.");
            publishService.MiddleMouseButtonDown();
        }

        public void MiddleButtonUp()
        {
            Log.Info("Releasing middle button.");
            publishService.MiddleMouseButtonUp();
        }

        public void MoveAndLeftClick(Point point, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and left clicking{1}", point, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.LeftMouseButtonClick();
        }

        public void MoveAndLeftDoubleClick(Point point, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and double left clicking{1}", point, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.LeftMouseButtonDoubleClick();
        }

        public void MoveAndMiddleClick(Point point, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and middle clicking{1}", point, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.MiddleMouseButtonClick();
        }

        public void MoveAndRightClick(Point point, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and right clicking{1}", point, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.RightMouseButtonClick();
        }

        public void MoveAndScrollWheelUp(Point point, int clicks, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and scrolling up by {1} clicks{2}", point, clicks, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.ScrollMouseWheelUp(clicks);
        }

        public void MoveAndScrollWheelDown(Point point, int clicks, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and scrolling down by {1} clicks{2}", point, clicks, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.ScrollMouseWheelDown(clicks);
        }

        public void MoveAndScrollWheelLeft(Point point, int clicks, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and scrolling left by {1} clicks{2}", point, clicks, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.ScrollMouseWheelLeft(clicks);
        }

        public void MoveAndScrollWheelRight(Point point, int clicks, bool sleep = false)
        {
            Log.DebugFormat("Moving cursor to point '{0}' and scrolling right by {1} clicks{2}", point, clicks, sleep ? " (with pauses between actions)" : null);
            publishService.MouseMouseToPoint(point);
            Thread.Sleep(DEFAULT_SLEEP_MS);
            publishService.ScrollMouseWheelRight(clicks);
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
            Log.Info("Pressing down right button.");
            publishService.RightMouseButtonDown();
        }

        public void RightButtonUp()
        {
            Log.Info("Releasing right button.");
            publishService.RightMouseButtonUp();
        }

        public void ScrollWheelAbsolute(int dx, int dy)
        {
            Log.DebugFormat("Scrolling wheel by <{0},{1}> in absolute units.", dx, dy);

            if (dx != 0 && dy != 0)
            {
                publishService.ScrollMouseWheelAbsoluteHorizontal(dx);
                Thread.Sleep(DEFAULT_SLEEP_MS);
                publishService.ScrollMouseWheelAbsoluteVertical(dy);
            }
            else if (dx != 0)
            {
                publishService.ScrollMouseWheelAbsoluteHorizontal(dx);
            }
            else if (dy != 0)
            {
                publishService.ScrollMouseWheelAbsoluteVertical(dy);
            }
        }

        #endregion
    }
}
