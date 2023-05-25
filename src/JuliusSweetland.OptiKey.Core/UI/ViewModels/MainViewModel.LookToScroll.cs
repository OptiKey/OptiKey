// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common.Enums;
using JuliusSweetland.OptiKey.Native.Common.Static;
using JuliusSweetland.OptiKey.Native.Common.Structs;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    partial class MainViewModel : ILookToScrollOverlayViewModel
    {
        private const int WheelUnitsPerClick = 120;
        private bool lookToScrolActive;

        private bool choosingLookToScrollBoundsTarget = false;
        private LookToScrollBounds lookToScrollBoundsWhenActivated = LookToScrollBounds.ScreenPoint;
        private Point pointLookToScrollBoundsTarget = new Point();
        private IntPtr windowLookToScrollBoundsTarget = IntPtr.Zero;
        private Rect rectLookToScrollBoundsTarget = Rect.Empty;
        private DateTime? lookToScrollLastUpdate = null;
        private Vector lookToScrollLeftoverScrollAmount = new Vector();

        #region ILookToScrollOverlayViewModel Members

        private bool isLookToScrollActive = false;
        public bool IsLookToScrollActive
        {
            get { return isLookToScrollActive; }
            private set { SetProperty(ref isLookToScrollActive, value); }
        }

        private Rect activeLookToScrollBounds = Rect.Empty;
        public Rect ActiveLookToScrollBounds
        {
            get { return activeLookToScrollBounds; }
            private set { SetProperty(ref activeLookToScrollBounds, value); }
        }

        private Rect activeLookToScrollDeadzone = Rect.Empty;
        public Rect ActiveLookToScrollDeadzone
        {
            get { return activeLookToScrollDeadzone; }
            private set { SetProperty(ref activeLookToScrollDeadzone, value); }
        }

        private Thickness activeLookToScrollMargins = new Thickness();
        public Thickness ActiveLookToScrollMargins
        {
            get { return activeLookToScrollMargins; }
            private set { SetProperty(ref activeLookToScrollMargins, value); }
        }

        private double opacity = 1;
        public double Opacity
        {
            get { return opacity; }
            private set { SetProperty(ref opacity, value); }
        }

        #endregion

        private void ToggleLookToScroll()
        {
            Log.Info("Look to scroll active key was selected.");

            lookToScrollBoundsWhenActivated = Settings.Default.LookToScrollBounds;

            if (keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey].Value.IsDownOrLockedDown())
            {
                // Turn off any locked (continuous) mouse actions
                ResetAndCleanupAfterMouseAction();
                SetCurrentMouseActionKey(null);

                Log.Info("Look to scroll is now active.");

                lookToScrollLeftoverScrollAmount = new Vector();
                ChooseLookToScrollBoundsTarget();
            }
            else
            {
                Log.Info("Look to scroll is no longer active.");
            }
        }

        private void ChooseLookToScrollBoundsTarget()
        {
            Log.Info("Choosing look to scroll bounds target.");

            choosingLookToScrollBoundsTarget = true;
            pointLookToScrollBoundsTarget = new Point();
            windowLookToScrollBoundsTarget = IntPtr.Zero;
            rectLookToScrollBoundsTarget = Rect.Empty;

            Action<bool> callback = success => 
            {
                if (!success)
                    keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey].Value = KeyDownStates.Up;

                TakeActionsUponLookToScrollStarted();
                choosingLookToScrollBoundsTarget = false;
            };
            
            switch (Settings.Default.LookToScrollBounds)
            {
                case LookToScrollBounds.ScreenPoint:
                    ChoosePointLookToScrollBoundsTarget(callback);
                    break;

                case LookToScrollBounds.ScreenCentred:
                    ChooseScreenLookToScrollBoundsTarget(callback);        
                    break;

                case LookToScrollBounds.Window:
                    ChooseWindowLookToScrollBoundsTarget(callback);
                    break;

                case LookToScrollBounds.Subwindow:
                    ChooseSubwindowLookToScrollBoundsTarget(callback);
                    break;

                case LookToScrollBounds.Custom:
                    ChooseCustomLookToScrollBoundsTarget(callback);
                    break;
            }
        }

        private void ChooseScreenLookToScrollBoundsTarget(Action<bool> callback)
        {
            Log.Info("Will use entire usable portion of the screen as the scroll bounds.");
            callback(true); // Always successful.
        }

        private void ChoosePointLookToScrollBoundsTarget(Action<bool> callback)
        {
            Log.Info("Choosing point on screen to use as the centre point for scrolling.");

            SetupFinalClickAction(point =>
            {
                if (point.HasValue)
                {
                    Log.InfoFormat("User chose point: {0}.", point.Value);
                    pointLookToScrollBoundsTarget = point.Value;

                    IntPtr hWnd = HideCursorAndGetHwndForFrontmostWindowAtPoint(point.Value);

                    if (hWnd == IntPtr.Zero)
                    {
                        Log.Info("No valid window at the point to bring to the front.");
                    }
                    else if (!PInvoke.SetForegroundWindow(hWnd))
                    {
                        Log.WarnFormat("Could not bring window at the point, {0}, to the front.", hWnd);
                    }
                    else
                    {
                        Log.InfoFormat("Brought window at the point, {0}, to the front.", hWnd);
                    }
                }

                ResetAndCleanupAfterMouseAction();
                callback(point.HasValue);
            }, suppressMagnification: true);
        }

        private void ChooseWindowLookToScrollBoundsTarget(Action<bool> callback)
        {
            Log.Info("Choosing a window to use as the scroll bounds.");

            SetupFinalClickAction(point =>
            {
                if (point.HasValue)
                {
                    Log.InfoFormat("User chose point: {0}.", point.Value);

                    IntPtr hWnd = HideCursorAndGetHwndForFrontmostWindowAtPoint(point.Value);

                    if (hWnd == IntPtr.Zero)
                    {
                        Log.Warn("Could not find a window at the chosen point!");
                        audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                    }
                    else
                    {
                        Log.InfoFormat("Selected window with HWND = {0} as the target.", hWnd);
                        windowLookToScrollBoundsTarget = hWnd;
                    }
                }

                ResetAndCleanupAfterMouseAction();
                callback(windowLookToScrollBoundsTarget != IntPtr.Zero);
            }, suppressMagnification: true);
        }

        private void ChooseSubwindowLookToScrollBoundsTarget(Action<bool> callback)
        {
            Log.Info("Choosing a rectangular region of a window to use as the scroll bounds.");

            Action finishUp = () =>
            {
                ResetAndCleanupAfterMouseAction();
                callback(windowLookToScrollBoundsTarget != IntPtr.Zero && !rectLookToScrollBoundsTarget.IsEmpty);
            };

            SetupFinalClickAction(firstCorner =>
            {
                if (firstCorner.HasValue)
                {
                    Log.InfoFormat("User chose {0} as the first corner.", firstCorner.Value);

                    IntPtr firstHWnd = HideCursorAndGetHwndForFrontmostWindowAtPoint(firstCorner.Value);

                    if (firstHWnd == IntPtr.Zero)
                    {
                        Log.Warn("No window was found at the first corner.");
                        audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                        finishUp();
                    }
                    else
                    {
                        Log.InfoFormat("Window {0} found at the first corner.", firstHWnd);

                        SetupFinalClickAction(secondCorner =>
                        {
                            if (secondCorner.HasValue)
                            {
                                Log.InfoFormat("User chose {0} as the second corner.", secondCorner.Value);

                                IntPtr secondHWnd = HideCursorAndGetHwndForFrontmostWindowAtPoint(secondCorner.Value);
                                var rect = new Rect(firstCorner.Value, secondCorner.Value);

                                if (secondHWnd == IntPtr.Zero)
                                {
                                    Log.Warn("No window was found at the second corner.");
                                    audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                                }
                                else if (secondHWnd != firstHWnd)
                                {
                                    Log.WarnFormat("Found a different window at the second corner: {0}.", secondHWnd);
                                    audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                                }
                                else if (!IsRectLargerThanDeadzone(rect))
                                {
                                    Log.Warn("The chosen rectangle is not large enough to accomodate the deadzone.");
                                    audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                                }
                                else
                                {
                                    Rect? windowBounds = GetWindowBounds(firstHWnd);
                                    if (windowBounds.HasValue)
                                    {
                                        // Express the rect relative to the top-left corner of the window so we can deal with possible
                                        // movement of the window later.
                                        rect.Offset(-windowBounds.Value.Left, -windowBounds.Value.Top);

                                        Log.InfoFormat("Selected rect {0} of window {1} as the look to scroll target.", rect, firstHWnd);

                                        windowLookToScrollBoundsTarget = firstHWnd;
                                        rectLookToScrollBoundsTarget = rect;
                                    }
                                    else
                                    {
                                        Log.Warn("Could not retrieve the bounds of the chosen window.");
                                        audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                                    }
                                }
                            }

                            finishUp();
                        }, suppressMagnification: true);
                    }
                }
                else
                {
                    finishUp();
                }
            }, finalClickInSeries: false, suppressMagnification: true);
        }

        private IntPtr GetHwndForFrontmostWindowAtPoint(Point point)
        {
            IntPtr shellWindow = PInvoke.GetShellWindow();

            Func<IntPtr, bool> criteria = hWnd => 
            {
                // Exclude the shell and Optikey windows
                if (hWnd == shellWindow || hWnd == mainWindowManipulationService.WindowHandle)
                {
                    return false;
                }

                // Exclude windows that aren't visible or that have been minimized.
                if (!PInvoke.IsWindowVisible(hWnd) || PInvoke.IsIconic(hWnd))
                {
                    return false;
                }

                // Exclude popup windows that have neither a frame like those used for regular windows
                // nor a frame like those used for dialog windows. This is intended to filter out things
                // like the lock screen, Start screen, and desktop wallpaper manager without filtering out
                // legitimate popup windows like "Open" and "Save As" dialogs as well as UWP apps.
                var style = Static.Windows.GetWindowStyle(hWnd);
                if ((style & WindowStyles.WS_POPUP) != 0 && 
                    (style & WindowStyles.WS_THICKFRAME) == 0 && 
                    (style & WindowStyles.WS_DLGFRAME) == 0)
                {
                    return false;
                }

                // Exclude transparent windows.
                var exStyle = Static.Windows.GetExtendedWindowStyle(hWnd);
                if (exStyle.HasFlag(ExtendedWindowStyles.WS_EX_TRANSPARENT))
                {
                    return false;
                }

                // Only include windows that contain the point.
                Rect? bounds = GetWindowBounds(hWnd);
                return bounds.HasValue && bounds.Value.Contains(point);
            };

            // Find the front-most top-level window that matches our criteria (expanding UWP apps into their CoreWindows).
            List<IntPtr> windows = Static.Windows.GetHandlesOfTopLevelWindows();
            windows = Static.Windows.ReplaceUWPTopLevelWindowsWithCoreWindowChildren(windows);
            windows = windows.Where(criteria).ToList();
            return Static.Windows.GetFrontmostWindow(windows);
        }

        private IntPtr HideCursorAndGetHwndForFrontmostWindowAtPoint(Point point)
        {
            // Make sure the cursor is hidden or else it may be picked as the front-most "window"!
            ShowCursor = false;

            return GetHwndForFrontmostWindowAtPoint(point);
        }

        private void ChooseCustomLookToScrollBoundsTarget(Action<bool> callback)
        {
            Log.Info("Choosing a rectangular region of the screen to use as the scroll bounds.");

            Action finishUp = () => 
            {
                ResetAndCleanupAfterMouseAction();
                callback(!rectLookToScrollBoundsTarget.IsEmpty);
            };

            SetupFinalClickAction(firstCorner => 
            {
                if (firstCorner.HasValue)
                {
                    Log.InfoFormat("User chose {0} as the first corner.", firstCorner.Value);

                    SetupFinalClickAction(secondCorner => 
                    {
                        if (secondCorner.HasValue)
                        {
                            Log.InfoFormat("User chose {0} as the second corner.", secondCorner.Value);

                            var rect = new Rect(firstCorner.Value, secondCorner.Value);

                            if (IsRectLargerThanDeadzone(rect))
                            {
                                Log.InfoFormat("Selected rect {0} as the look to scroll target.", rect);
                                rectLookToScrollBoundsTarget = rect;
                            }
                            else
                            {
                                Log.Warn("The chosen rectangle is not large enough to accomodate the deadzone.");
                                audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                            }
                        }

                        finishUp();
                    }, suppressMagnification: true);
                }
                else
                {
                    finishUp();
                }
            }, finalClickInSeries: false, suppressMagnification: true);
        }

        private bool IsRectLargerThanDeadzone(Rect rect)
        {
            return (
                rect.Width > Settings.Default.LookToScrollHorizontalDeadzone &&
                rect.Height > Settings.Default.LookToScrollVerticalDeadzone
            );
        }

        private void TakeActionsUponLookToScrollStarted()
        {
            switch (lookToScrollBoundsWhenActivated)
            {
                case LookToScrollBounds.Window:
                case LookToScrollBounds.Subwindow:
                    if (Settings.Default.LookToScrollBringWindowToFrontWhenActivated)
                    {
                        BringLookToScrollWindowBoundsTargetToFront();
                    }
                    break;
            }

            if (Settings.Default.LookToScrollCentreMouseWhenActivated)
            {
                CentreMouseInsideLookToScrollDeadzone();
            }

            SelectionMode = SelectionModes.Keys;
        }

        private void CentreMouseInsideLookToScrollDeadzone()
        {
            Log.Info("Moving mouse to centre of look to scroll deadzone.");

            Rect? bounds = GetCurrentLookToScrollBoundsRect();
            if (bounds.HasValue)
            {
                Action reinstateModifiers = () => { };
                if (keyStateService.SimulateKeyStrokes
                    && Settings.Default.SuppressModifierKeysForAllMouseActions)
                {
                    reinstateModifiers = keyStateService.ReleaseModifiers(Log);
                }

                mouseOutputService.MoveTo(GetCurrentLookToScrollCentrePoint(bounds.Value));

                reinstateModifiers();
            }
            else
            {
                Log.Warn("Could not get look to scroll bounds rect. Leaving mouse alone.");
            }
        }

        private void BringLookToScrollWindowBoundsTargetToFront()
        {
            Log.InfoFormat("Bringing look to scroll target window {0} to front.", windowLookToScrollBoundsTarget);

            if (!PInvoke.SetForegroundWindow(windowLookToScrollBoundsTarget))
            {
                Log.Warn("Could not bring window to front.");
            }
        }

        private void UpdateLookToScroll(Point position)
        {
            var thisUpdate = DateTime.Now;

            Rect bounds;
            Point centre;
            bool active = ShouldUpdateLookToScroll(position, out bounds, out centre);

            if (active)
            {
                Log.DebugFormat("Updating look to scroll using position: {0}.", position);
                Log.DebugFormat("Current look to scroll bounds rect is: {0}.", bounds);
                Log.DebugFormat("Current look to scroll centre point is: {0}.", centre);

                Opacity = (4 * ((position - centre).Length - activeLookToScrollDeadzone.Height / 2)
                    / activeLookToScrollBounds.Height).Clamp(0.1, 1);

                Vector velocity = CalculateLookToScrollVelocity(position, centre);

                // Convert the velocity from clicks per second to mouse wheel units per second.
                velocity *= WheelUnitsPerClick;

                double interval = (thisUpdate - lookToScrollLastUpdate.Value).TotalSeconds.CoerceToUpperLimit(0.1);
                Vector scrollAmount = velocity * interval;

                // Carry over any unused scrolling from last update.
                scrollAmount += lookToScrollLeftoverScrollAmount;


                PerformLookToScroll(scrollAmount);
            }

            if (lookToScrolActive || active)
                UpdateLookToScrollOverlayProperties(active, bounds, centre);

            lookToScrolActive = active;
            lookToScrollLastUpdate = thisUpdate;
        }

        private bool ShouldUpdateLookToScroll(Point position, out Rect bounds, out Point centre)
        {
            bounds = Rect.Empty;
            centre = new Point();

            if (!keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey].Value.IsDownOrLockedDown() ||
                keyStateService.KeyDownStates[KeyValues.SleepKey].Value.IsDownOrLockedDown() ||
                choosingLookToScrollBoundsTarget ||
                !lookToScrollLastUpdate.HasValue)
            {
                return false;
            }

            Rect? boundsContainer = GetCurrentLookToScrollBoundsRect();

            if (!boundsContainer.HasValue)
            {
                Log.Info("Look to scroll bounds is no longer valid. Deactivating look to scroll.");

                keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey].Value = KeyDownStates.Up;
                keyStateService.KeyDownStates[KeyValues.LookToScrollBoundsKey].Value = KeyDownStates.Up;

                return false;
            }

            bounds = boundsContainer.Value;
            centre = GetCurrentLookToScrollCentrePoint(bounds);

            // If using a window or portion of it as the bounds target, only scroll while pointing _at_ that window, 
            // not while pointing at another window on top of it.
            switch (lookToScrollBoundsWhenActivated)
            {
                case LookToScrollBounds.Window:
                case LookToScrollBounds.Subwindow:
                    if (GetHwndForFrontmostWindowAtPoint(position) != windowLookToScrollBoundsTarget)
                    {
                        return false;
                    }
                    break;
            }

            if (position.ToKeyValue(pointToKeyValueMap) != null)
            {
                return false;
            }

            return bounds.Contains(position);
        }

        private Rect? GetCurrentLookToScrollBoundsRect()
        {
            Rect? bounds = null;

            switch (lookToScrollBoundsWhenActivated)
            {
                case LookToScrollBounds.ScreenPoint:
                case LookToScrollBounds.ScreenCentred:
                    bounds = IsMainWindowDocked() 
                        ? FindLargestGapBetweenScreenAndMainWindow() 
                        : GetPrimaryScreenBoundsInPixels();
                    break;

                case LookToScrollBounds.Window:
                    bounds = GetWindowBounds(windowLookToScrollBoundsTarget);  
                    break;

                case LookToScrollBounds.Subwindow:
                    bounds = GetSubwindowBoundsOnScreen(windowLookToScrollBoundsTarget, rectLookToScrollBoundsTarget);
                    break;

                case LookToScrollBounds.Custom:
                    bounds = rectLookToScrollBoundsTarget;
                    break;
            }

            return bounds;
        }

        // Finds the largest rectangular region outside the main window
        // Used to identify the main screen region remaining after docked app takes up space
        private Rect FindLargestGapBetweenScreenAndMainWindow()
        {
            Rect screen = GetPrimaryScreenBoundsInPixels();
            Rect window = GetMainWindowBoundsInPixels();

            var above = new Rect { X = screen.Left, Y = screen.Top, Width = screen.Width, Height = window.Top >= screen.Top ? window.Top - screen.Top : 0 };
            var below = new Rect { X = screen.Left, Y = window.Bottom, Width = screen.Width, Height = screen.Bottom >= window.Bottom ? screen.Bottom - window.Bottom : 0 };
            var left = new Rect { X = screen.Left, Y = screen.Top, Width = window.Left >= screen.Left ? window.Left - screen.Left : 0, Height = screen.Height };
            var right = new Rect { X = window.Right, Y = screen.Top, Width = screen.Right >= window.Right ? screen.Right - window.Right : 0, Height = screen.Height };
            
            return new Rect[] { above, below, left, right }.OrderByDescending(rect => rect.CalculateArea()).First();
        }

        private Rect? GetWindowBounds(IntPtr hWnd)
        {
            if (!PInvoke.IsWindow(hWnd))
            {
                Log.WarnFormat("{0} does not or no longer points to a valid window.", hWnd);
                return null;
            }

            RECT rawRect;

            if (PInvoke.DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out rawRect, Marshal.SizeOf<RECT>()) != 0)
            {
                Log.WarnFormat("Failed to get bounds of window {0} using DwmGetWindowAttribute. Falling back to GetWindowRect.", hWnd);

                if (!PInvoke.GetWindowRect(hWnd, out rawRect))
                {
                    Log.WarnFormat("Failed to get bounds of window {0} using GetWindowRect.", hWnd);
                    return null;
                }
            }

            return new Rect
            {
                X = rawRect.Left,
                Y = rawRect.Top,
                Width = rawRect.Right - rawRect.Left,
                Height = rawRect.Bottom - rawRect.Top
            };
        }

        private Rect? GetSubwindowBoundsOnScreen(IntPtr hWnd, Rect relativeBounds)
        {
            Rect? windowBounds = GetWindowBounds(hWnd);
            if (windowBounds.HasValue)
            {
                // Express the relative bounds in virtual screen-space again now that we know the location of
                // the window's top-left corner.
                Rect subwindowBounds = relativeBounds;
                subwindowBounds.Offset(windowBounds.Value.Left, windowBounds.Value.Top);

                // Make sure the subwindow bounds are fully contained within the window since the window may have 
                // shrunk since it was chosen.
                subwindowBounds.Intersect(windowBounds.Value);

                return subwindowBounds;
            }
            else
            {
                return null;
            }
        }

        private Point GetCurrentLookToScrollCentrePoint(Rect bounds)
        {
            switch (lookToScrollBoundsWhenActivated)
            {
                case LookToScrollBounds.ScreenPoint:
                    return pointLookToScrollBoundsTarget;

                default:
                    return bounds.CalculateCentre();
            }
        }

        private Vector CalculateLookToScrollVelocity(Point current, Point centre)
        {
            var baseSpeed = Settings.Default.LookToScrollSpeed == LookToScrollSpeeds.Fast
                ? 0.3 : Settings.Default.LookToScrollSpeed == LookToScrollSpeeds.Medium
                ? 0.1 : 0.03;
            var acceleration = Settings.Default.LookToScrollSpeed == LookToScrollSpeeds.Fast
                ? 0.3 : Settings.Default.LookToScrollSpeed == LookToScrollSpeeds.Medium
                ? 0.1 : 0.03;

            var velocity = new Vector { X = 0, Y = 0 };

            // Horizontal velocity only applies in horizontal (duh!), cross, and free modes.
            switch (Settings.Default.LookToScrollMode)
            {
                case LookToScrollModes.Horizontal:
                case LookToScrollModes.Cross:
                case LookToScrollModes.Free:
                    velocity.X = CalculateLookToScrollVelocity(
                        current.X, 
                        centre.X,
                        Settings.Default.LookToScrollHorizontalDeadzone, 
                        baseSpeed, 
                        acceleration
                    );
                    break;
            }

            // Vertical velocity only applies in vertical (duh!), cross, and free modes.
            switch (Settings.Default.LookToScrollMode)
            {
                case LookToScrollModes.Vertical:
                case LookToScrollModes.Cross:
                case LookToScrollModes.Free:
                    velocity.Y = CalculateLookToScrollVelocity(
                        current.Y,
                        centre.Y,
                        Settings.Default.LookToScrollVerticalDeadzone,
                        baseSpeed,
                        acceleration
                    );
                    break;
            }

            // Cross mode is like free mode except it only permits scrolling in one direction at a time.
            // So, if there'd be non-zero velocity along both axes, zero it out to prevent any scrolling.
            if (Settings.Default.LookToScrollMode == LookToScrollModes.Cross && velocity.X != 0 && velocity.Y != 0)
            {
                velocity.X = velocity.Y = 0;
            }

            Log.DebugFormat("Current scrolling velocity is: {0}.", velocity);

            return velocity;
        }

        private double CalculateLookToScrollVelocity(
            double current, 
            double centre, 
            double deadzone, 
            double baseSpeed, 
            double acceleration)
        {
            // Calculate the direction and distance from the centre to the current value. 
            double signedDistance = current - centre;
            double sign = Math.Sign(signedDistance);
            double distance = Math.Abs(signedDistance);

            // Remove the deadzone.
            distance -= deadzone;
            if (distance < 0)
            {
                return 0;
            }

            // Calculate total speed using base speed and distance-based acceleration.
            double speed = baseSpeed + distance * acceleration;

            // Give the speed the correct direction.
            return sign * speed;
        }

        private void PerformLookToScroll(Vector scrollAmount)
        {
            int dx = (int)scrollAmount.X;
            int dy = (int)scrollAmount.Y;

            // Carry over any unused scroll amount into the next update to make sure we don't lose any due to
            // the truncation to int or the scroll increment requirement.
            lookToScrollLeftoverScrollAmount.X = scrollAmount.X - dx;
            lookToScrollLeftoverScrollAmount.Y = scrollAmount.Y - dy;

            Log.DebugFormat("Storing {0} leftover scroll amount for next update.", lookToScrollLeftoverScrollAmount);

            Action reinstateModifiers = () => { };
            if (keyStateService.SimulateKeyStrokes && Settings.Default.SuppressModifierKeysForAllMouseActions)
            {
                reinstateModifiers = keyStateService.ReleaseModifiers(Log);
            }

            mouseOutputService.ScrollWheelAbsolute(dx, -dy);

            reinstateModifiers();
        }

        private void UpdateLookToScrollOverlayProperties(bool active, Rect bounds, Point centre)
        {
            int hDeadzone = Settings.Default.LookToScrollHorizontalDeadzone;
            int vDeadzone = Settings.Default.LookToScrollVerticalDeadzone;

            var deadzone = new Rect
            {
                X = centre.X - hDeadzone,
                Y = centre.Y - vDeadzone,
                Width = hDeadzone * 2,
                Height = vDeadzone * 2,
            };

            IsLookToScrollActive = active;
            ActiveLookToScrollBounds = Graphics.PixelsToDips(bounds);
            ActiveLookToScrollDeadzone = Graphics.PixelsToDips(deadzone);
            ActiveLookToScrollMargins = Graphics.PixelsToDips(bounds.CalculateMarginsAround(deadzone));
        }

        private Action SuspendLookToScrollWhileChoosingPointForMouse()
        {
            Action resumeAction = () => { };

            NotifyingProxy<KeyDownStates> activeKey = keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey];
            KeyDownStates originalState = activeKey.Value;

            // Make sure look to scroll is currently active. Otherwise, there's nothing to suspend or resume.
            if (originalState.IsDownOrLockedDown())
            {
                // Force scrolling to stop by releasing the LookToScrollActiveKey.
                activeKey.Value = KeyDownStates.Up;

                // If configured to resume afterwards, just reapply the original state of the key so the user doesn't have 
                // to rechoose the bounds. Otherwise, the user will have to press the key themselves and potentially rechoose 
                // the bounds (depending on the state of the bounds key). 
                if (Settings.Default.LookToScrollResumeAfterChoosingPointForMouse)
                {
                    Log.Info("Look to scroll has suspended.");

                    resumeAction = async () =>
                    {
                        //Give time for click to process before resuming
                        await Task.Delay(200);
                        activeKey.Value = originalState;

                        if (Settings.Default.LookToScrollCentreMouseWhenActivated)
                        {
                            CentreMouseInsideLookToScrollDeadzone();
                        }

                        Log.Info("Look to scroll has resumed.");
                    };
                }
                else
                {
                    Log.Info("Look to scroll has been suspended and will not automatically resume.");
                }
            }

            return resumeAction;
        }

        private void DeactivateLookToScrollUponSwitchingKeyboards()
        {
            if (Settings.Default.LookToScrollDeactivateUponSwitchingKeyboards)
            {
                keyStateService.KeyDownStates[KeyValues.LookToScrollActiveKey].Value = KeyDownStates.Up;
                Log.Info("Look to scroll is no longer active.");
            }
        }

        private Rect GetPrimaryScreenBoundsInPixels()
        {
            return new Rect
            {
                X = 0,
                Y = 0,
                Width = Graphics.PrimaryScreenWidthInPixels,
                Height = Graphics.PrimaryScreenHeightInPixels,
            };
        }


        private Rect GetMainWindowBoundsInPixels()
        {
            return Graphics.DipsToPixels(mainWindowManipulationService.WindowBounds);
        }

        private bool IsMainWindowDocked()
        {
            return mainWindowManipulationService.WindowState == WindowStates.Docked;
        }
    }
}
