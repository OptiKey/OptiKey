using System;
using System.Windows;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowStatePersistenceService
    {
        #region Private Member Vars

        private readonly Func<double> getWindowTopSetting;
        private readonly Action<double> setWindowTopSetting;
        private readonly Func<double> getWindowLeftSetting;
        private readonly Action<double> setWindowLeftSetting;
        private readonly Func<double> getWindowHeightSetting;
        private readonly Action<double> setWindowHeightSetting;
        private readonly Func<double> getWindowWidthSetting;
        private readonly Action<double> setWindowWidthSetting;
        private readonly Func<WindowState> getWindowStateSetting;
        private readonly Action<WindowState> setWindowStateSetting;
        private readonly Settings settings;

        private double windowTop;
        private double windowLeft;
        private double windowHeight;
        private double windowWidth;
        private WindowState windowState;

        #endregion

        #region Ctor

        internal WindowStatePersistenceService(
            Func<double> getWindowTopSetting,
            Action<double> setWindowTopSetting,
            Func<double> getWindowLeftSetting,
            Action<double> setWindowLeftSetting,
            Func<double> getWindowHeightSetting,
            Action<double> setWindowHeightSetting,
            Func<double> getWindowWidthSetting,
            Action<double> setWindowWidthSetting,
            Func<WindowState> getWindowStateSetting,
            Action<WindowState> setWindowStateSetting,
            Settings settings)
        {
            this.getWindowTopSetting = getWindowTopSetting;
            this.setWindowTopSetting = setWindowTopSetting;
            this.getWindowLeftSetting = getWindowLeftSetting;
            this.setWindowLeftSetting = setWindowLeftSetting;
            this.getWindowHeightSetting = getWindowHeightSetting;
            this.setWindowHeightSetting = setWindowHeightSetting;
            this.getWindowWidthSetting = getWindowWidthSetting;
            this.setWindowWidthSetting = setWindowWidthSetting;
            this.getWindowStateSetting = getWindowStateSetting;
            this.setWindowStateSetting = setWindowStateSetting;
            this.settings = settings;

            //Load the settings
            Load();

            //Size it to fit the current screen
            SizeToFit();

            //Move the window at least partially into view
            MoveIntoView();
        }

        #endregion

        #region Properties

        public double WindowTop
        {
            get { return windowTop; }
            set { windowTop = value; }
        }

        public double WindowLeft
        {
            get { return windowLeft; }
            set { windowLeft = value; }
        }

        public double WindowHeight
        {
            get { return windowHeight; }
            set { windowHeight = value; }
        }

        public double WindowWidth
        {
            get { return windowWidth; }
            set { windowWidth = value; }
        }

        public WindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; }
        }

        #endregion

        private void Load()
        {
            windowTop = getWindowTopSetting();
            windowLeft = getWindowLeftSetting();
            windowHeight = getWindowHeightSetting();
            windowWidth = getWindowWidthSetting();
            windowState = getWindowStateSetting();
        }

        public void SizeToFit()
        {
            var virtualScreenHeightInDpi = SystemParameters.VirtualScreenHeight / Graphics.ScalingFactorY;
            if (windowHeight > virtualScreenHeightInDpi)
            {
                windowHeight = virtualScreenHeightInDpi;
            }

            var virtualScreenWidthInDpi = SystemParameters.VirtualScreenWidth / Graphics.ScalingFactorX;
            if (windowWidth > virtualScreenWidthInDpi)
            {
                windowWidth = virtualScreenWidthInDpi;
            }
        }

        public void MoveIntoView()
        {
            var virtualScreenTopInDpi = SystemParameters.VirtualScreenTop / Graphics.ScalingFactorY;
            var virtualScreenHeightInDpi = SystemParameters.VirtualScreenHeight / Graphics.ScalingFactorY;
            
            if (windowTop < virtualScreenTopInDpi)
            {
                windowTop = virtualScreenTopInDpi;
            }
            else if (windowTop + windowHeight > virtualScreenTopInDpi + virtualScreenHeightInDpi)
            {
                windowTop = virtualScreenTopInDpi + virtualScreenHeightInDpit - windowHeight;
            }

            var virtualScreenLeftInDpi = SystemParameters.VirtualScreenLeft / Graphics.ScalingFactorX;
            var virtualScreenWidthInDpi = SystemParameters.VirtualScreenWidth / Graphics.ScalingFactorX;

            if (windowLeft < virtualScreenLeftInDpi)
            {
                windowLeft = virtualScreenLeftInDpi;
            }
            else if (windowLeft + windowWidth > virtualScreenLeftInDpi + virtualScreenWidthInDpi)
            {
                windowLeft = virtualScreenLeftInDpi + virtualScreenWidthInDpi - windowWidth;
            }
        }

        public void Save()
        {
            if (windowState != WindowState.Minimized)
            {
                setWindowTopSetting(windowTop);
                setWindowLeftSetting(windowLeft);
                setWindowHeightSetting(windowHeight);
                setWindowWidthSetting(windowWidth);
                setWindowStateSetting(windowState);

                settings.Save();
            }
        }
    }
}
