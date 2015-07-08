using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards;
using Size = JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Size;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    public partial class MainViewModel
    {
        public void AttachServiceEventHandlers()
        {
            Log.Debug("AttachServiceEventHandlers called.");

            if (errorNotifyingServices != null)
            {
                errorNotifyingServices.ForEach(s => s.Error += HandleServiceError);
            }

            inputService.PointsPerSecond += (o, value) => { PointsPerSecond = value; };

            inputService.CurrentPosition += (o, tuple) =>
            {
                CurrentPositionPoint = tuple.Item1;
                CurrentPositionKey = tuple.Item2;
            };

            inputService.SelectionProgress += (o, progress) =>
            {
                if (progress.Item1 == null
                    && progress.Item2 == 0)
                {
                    ResetSelectionProgress(); //Reset all keys
                }
                else if (progress.Item1 != null)
                {
                    if (SelectionMode == SelectionModes.Key
                        && progress.Item1.Value.KeyValue != null)
                    {
                        keyboardService.KeySelectionProgress[progress.Item1.Value.KeyValue.Value] =
                            new NotifyingProxy<double>(progress.Item2);
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PointSelectionProgress = new Tuple<Point, double>(progress.Item1.Value.Point, progress.Item2);
                    }
                }
            };

            inputService.Selection += (o, value) =>
            {
                Log.Debug("Selection event received from InputService.");

                SelectionResultPoints = null; //Clear captured points from previous SelectionResult event

                if (SelectionMode == SelectionModes.Key
                    && value.KeyValue != null)
                {
                    if (!capturingStateManager.CapturingMultiKeySelection)
                    {
                        audioService.PlaySound(Settings.Default.KeySelectionSoundFile, Settings.Default.KeySelectionSoundVolume);
                    }

                    if (KeySelection != null)
                    {
                        Log.DebugFormat("Firing KeySelection event with KeyValue '{0}'", value.KeyValue.Value);
                        KeySelection(this, value.KeyValue.Value);
                    }
                }
                else if (SelectionMode == SelectionModes.Point)
                {
                    if (PointSelection != null)
                    {
                        PointSelection(this, value.Point);

                        if (nextPointSelectionAction != null)
                        {
                            Log.DebugFormat("Executing nextPointSelectionAction delegate with point '{0}'", value.Point);
                            nextPointSelectionAction(value.Point);
                        }
                    }
                }
            };

            inputService.SelectionResult += (o, tuple) =>
            {
                Log.Debug("SelectionResult event received from InputService.");

                var points = tuple.Item1;
                var singleKeyValue = tuple.Item2 != null || tuple.Item3 != null
                    ? new KeyValue { FunctionKey = tuple.Item2, String = tuple.Item3 }
                    : (KeyValue?)null;
                var multiKeySelection = tuple.Item4;

                SelectionResultPoints = points; //Store captured points from SelectionResult event (displayed for debugging)

                if (SelectionMode == SelectionModes.Key
                    && (singleKeyValue != null || (multiKeySelection != null && multiKeySelection.Any())))
                {
                    KeySelectionResult(singleKeyValue, multiKeySelection);
                }
                else if (SelectionMode == SelectionModes.Point)
                {
                    //SelectionResult event has no real meaning when dealing with point selection
                }
            };

            inputService.PointToKeyValueMap = pointToKeyValueMap;
            inputService.SelectionMode = SelectionMode;
        }
        
        private void KeySelectionResult(KeyValue? singleKeyValue, List<string> multiKeySelection)
        {
            //Single key string
            if (singleKeyValue != null
                && !string.IsNullOrEmpty(singleKeyValue.Value.String))
            {
                Log.DebugFormat("KeySelectionResult received with string value '{0}'", singleKeyValue.Value.String.ConvertEscapedCharsToLiterals());
                outputService.ProcessSingleKeyText(singleKeyValue.Value.String);
            }

            //Single key function key
            if (singleKeyValue != null
                && singleKeyValue.Value.FunctionKey != null)
            {
                Log.DebugFormat("KeySelectionResult received with function key value '{0}'", singleKeyValue.Value.FunctionKey);
                HandleFunctionKeySelectionResult(singleKeyValue.Value);
            }

            //Multi key selection
            if (multiKeySelection != null
                && multiKeySelection.Any())
            {
                Log.DebugFormat("KeySelectionResult received with '{0}' multiKeySelection results", multiKeySelection.Count);
                outputService.ProcessMultiKeyTextAndSuggestions(multiKeySelection);
            }
        }

        private void HandleFunctionKeySelectionResult(KeyValue singleKeyValue)
        {
            if (singleKeyValue.FunctionKey != null)
            {
                keyboardService.ProgressKeyDownState(singleKeyValue);

                var currentKeyboard = Keyboard;

                switch (singleKeyValue.FunctionKey.Value)
                {
                    case FunctionKeys.AddToDictionary:
                        AddTextToDictionary();
                        break;

                    case FunctionKeys.AlphaKeyboard:
                        Log.Debug("Changing keyboard to Alpha.");
                        Keyboard = new Alpha();
                        break;

                    case FunctionKeys.ArrangeWindowsHorizontally:
                        Log.Debug("Arranging windows horizontally.");
                        mainWindowManipulationService.ArrangeWindowsHorizontally();
                        break;

                    case FunctionKeys.ArrangeWindowsMaximised:
                        Log.Debug("Arranging windows maximised.");
                        mainWindowManipulationService.ArrangeWindowsMaximised();
                        break;

                    case FunctionKeys.ArrangeWindowsVertically:
                        Log.Debug("Arranging windows vertically.");
                        mainWindowManipulationService.ArrangeWindowsVertically();
                        break;

                    case FunctionKeys.Diacritic1Keyboard:
                        Log.Debug("Changing keyboard to Diacritic1.");
                        Keyboard = new Diacritic1();
                        break;

                    case FunctionKeys.Diacritic2Keyboard:
                        Log.Debug("Changing keyboard to Diacritic2.");
                        Keyboard = new Diacritic2();
                        break;

                    case FunctionKeys.Diacritic3Keyboard:
                        Log.Debug("Changing keyboard to Diacritic3.");
                        Keyboard = new Diacritic3();
                        break;

                    case FunctionKeys.BackFromKeyboard:
                        Log.Debug("Navigating back from keyboard.");
                        var navigableKeyboard = Keyboard as IBackAction;
                        if (navigableKeyboard != null && navigableKeyboard.BackAction != null)
                        {
                            navigableKeyboard.BackAction();
                        }
                        else
                        {
                            Keyboard = new Alpha();
                        }
                        break;

                    case FunctionKeys.Calibrate:
                        if (CalibrationService != null)
                        {
                            Log.Debug("Calibrate requested.");

                            var previousKeyboard = Keyboard;
                            
                            var question = CalibrationService.CanBeCompletedWithoutManualIntervention
                            ? "Are you sure you would like to re-calibrate?"
                            : "Calibration cannot be completed without manual intervention, e.g. having to use a mouse. You may be stuck in the calibration process if you cannot manually interact with your computer.\nAre you sure you would like to re-calibrate?";
                            
                            Keyboard = new YesNoQuestion(
                                question,
                                () =>
                                {
                                    inputService.RequestSuspend();
                                    Keyboard = previousKeyboard;
                                    CalibrateRequest.Raise(new NotificationWithCalibrationResult(), calibrationResult =>
                                    {
                                        if (calibrationResult.Success)
                                        {
                                            audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                                            RaiseToastNotification("Success", calibrationResult.Message, NotificationTypes.Normal, () => { inputService.RequestResume(); });
                                        }
                                        else
                                        {
                                            audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                                            RaiseToastNotification("Uh-oh!", calibrationResult.Exception != null
                                                    ? calibrationResult.Exception.Message
                                                    : calibrationResult.Message ?? "Something went wrong, but I don't know what - please check the logs", 
                                                NotificationTypes.Error, 
                                                () => { inputService.RequestResume(); });
                                        }
                                    });
                                },
                                () =>
                                {
                                    Keyboard = previousKeyboard;
                                });
                        }
                        break;

                    case FunctionKeys.Currencies1Keyboard:
                        Log.Debug("Changing keyboard to Currencies1.");
                        Keyboard = new Currencies1();
                        break;

                    case FunctionKeys.Currencies2Keyboard:
                        Log.Debug("Changing keyboard to Currencies2.");
                        Keyboard = new Currencies2();
                        break;

                    case FunctionKeys.DecreaseOpacity:
                        Log.Debug("Decreasing opacity.");
                        mainWindowManipulationService.DecreaseOpacity();
                        break;

                    case FunctionKeys.ExpandToBottom:
                        Log.DebugFormat("Expanding to bottom by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToBottom(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToBottomAndLeft:
                        Log.DebugFormat("Expanding to bottom and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToBottomAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToBottomAndRight:
                        Log.DebugFormat("Expanding to bottom and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToBottomAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToLeft:
                        Log.DebugFormat("Expanding to left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToRight:
                        Log.DebugFormat("Expanding to right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToTop:
                        Log.DebugFormat("Expanding to top by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToTop(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToTopAndLeft:
                        Log.DebugFormat("Expanding to top and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToTopAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ExpandToTopAndRight:
                        Log.DebugFormat("Expanding to top and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ExpandToTopAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.IncreaseOpacity:
                        Log.Debug("Increasing opacity.");
                        mainWindowManipulationService.IncreaseOpacity();
                        break;

                    case FunctionKeys.MaximiseSize:
                        Log.Debug("Maximising size.");
                        mainWindowManipulationService.Maximise();
                        break;
                        
                    case FunctionKeys.MenuKeyboard:
                        Log.Debug("Changing keyboard to Menu.");
                        Keyboard = new Menu(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToBottomAndLeftBoundaries:
                        Log.Debug("Minimising to bottom and left boundaries.");
                        mainWindowManipulationService.MinimiseToBottomAndLeftBoundaries();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToBottomAndRightBoundaries:
                        Log.Debug("Minimising to bottom and right boundaries.");
                        mainWindowManipulationService.MinimiseToBottomAndRightBoundaries();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToMiddleOfBottomBoundary:
                        Log.Debug("Minimising to bottom boundary.");
                        mainWindowManipulationService.MinimiseToMiddleOfBottomBoundary();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToMiddleOfLeftBoundary:
                        Log.Debug("Minimising to left boundary.");
                        mainWindowManipulationService.MinimiseToMiddleOfLeftBoundary();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToMiddleOfRightBoundary:
                        Log.Debug("Minimising to right boundary.");
                        mainWindowManipulationService.MinimiseToMiddleOfRightBoundary();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToTopAndLeftBoundaries:
                        Log.Debug("Minimising to top and left boundaries.");
                        mainWindowManipulationService.MinimiseToTopAndLeftBoundaries();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToTopAndRightBoundaries:
                        Log.Debug("Minimising to top and right boundaries.");
                        mainWindowManipulationService.MinimiseToTopAndRightBoundaries();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.MinimiseToMiddleOfTopBoundary:
                        Log.Debug("Minimising to top boundary.");
                        mainWindowManipulationService.MinimiseToMiddleOfTopBoundary();
                        Log.Debug("Changing keyboard to Minimised.");
                        Keyboard = new Minimised(() => Keyboard = currentKeyboard);
                        break;
                    
                    case FunctionKeys.MouseKeyboard:
                        Log.Debug("Changing keyboard to Mouse.");
                        if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown()
                            && Settings.Default.DisableKeyboardSimulationWhileMouseKeyboardIsOpen)
                        {
                            var lastSimulateKeyStrokesValue = keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value;
                            var lastLeftShiftValue = keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value;
                            var lastLeftCtrlValue = keyboardService.KeyDownStates[KeyValues.LeftCtrlKey].Value;
                            var lastLeftWinValue = keyboardService.KeyDownStates[KeyValues.LeftWinKey].Value;
                            var lastLeftAltValue = keyboardService.KeyDownStates[KeyValues.LeftAltKey].Value;
                            keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value = KeyDownStates.Up;
                            Keyboard = new Mouse(() =>
                            {
                                keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value = lastSimulateKeyStrokesValue;
                                keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value = lastLeftShiftValue;
                                keyboardService.KeyDownStates[KeyValues.LeftCtrlKey].Value = lastLeftCtrlValue;
                                keyboardService.KeyDownStates[KeyValues.LeftWinKey].Value = lastLeftWinValue;
                                keyboardService.KeyDownStates[KeyValues.LeftAltKey].Value = lastLeftAltValue;
                                Keyboard = currentKeyboard;
                            });
                        }
                        else
                        {
                            Keyboard = new Mouse(() => Keyboard = currentKeyboard);
                        }
                        break;

                    case FunctionKeys.MouseDoubleLeftClick:
                        Log.Debug("Mouse double left click selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseDoubleClickSoundFile, Settings.Default.MouseDoubleClickSoundVolume);
                                    outputService.LeftMouseButtonDoubleClick(fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }
                            
                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseDrag:
                        Log.Debug("Mouse drag selected.");
                        SetupFinalClickAction(firstFinalPoint =>
                        {
                            if (firstFinalPoint != null)
                            {
                                //This class reacts to the point selection event AFTER the MagnifyPopup reacts to it.
                                //This means that if the MagnifyPopup sets the nextPointSelectionAction from the
                                //MagnifiedPointSelectionAction then it will be called immediately i.e. for the same point.
                                //The workaround is to set the nextPointSelectionAction to a lambda which sets the NEXT
                                //nextPointSelectionAction. This means the immediate call to the lambda just sets up the
                                //delegate for the subsequent call.
                                nextPointSelectionAction = repeatFirstClickOrSecondClickAction =>
                                {
                                    Action<Point> deferIfMagnifyingElseDoNow = repeatFirstClickOrSecondClickPoint =>
                                    {
                                        Action<Point?> secondFinalClickAction = secondFinalPoint =>
                                        {
                                            if (secondFinalPoint != null)
                                            {
                                                Action<Point, Point> simulateDrag = (fp1, fp2) =>
                                                {
                                                    audioService.PlaySound(Settings.Default.MouseClickSoundFile, Settings.Default.MouseClickSoundVolume);
                                                    outputService.LeftMouseButtonDown(fp1);
                                                    audioService.PlaySound(Settings.Default.MouseClickSoundFile, Settings.Default.MouseClickSoundVolume);
                                                    outputService.LeftMouseButtonUp(fp2);
                                                };

                                                lastMouseActionStateManager.LastMouseAction =
                                                    () => simulateDrag(firstFinalPoint.Value, secondFinalPoint.Value);
                                                simulateDrag(firstFinalPoint.Value, secondFinalPoint.Value);
                                            }

                                            ResetAndCleanupAfterMouseAction();
                                        };

                                        if (keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value.IsDownOrLockedDown())
                                        {
                                            ShowCursor = false; //See MouseLeftClick case for explanation of this
                                            MagnifiedPointSelectionAction = secondFinalClickAction;
                                            MagnifyAtPoint = repeatFirstClickOrSecondClickPoint;
                                            ShowCursor = true;
                                        }
                                        else
                                        {
                                            secondFinalClickAction(repeatFirstClickOrSecondClickPoint);
                                        }

                                        nextPointSelectionAction = null;
                                    };

                                    if (keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value.IsDownOrLockedDown())
                                    {
                                        nextPointSelectionAction = deferIfMagnifyingElseDoNow;
                                    }
                                    else
                                    {
                                        deferIfMagnifyingElseDoNow(repeatFirstClickOrSecondClickAction);
                                    }
                                };
                            }
                            else
                            {
                                //Reset and clean up if we are not continuing to 2nd point
                                SelectionMode = SelectionModes.Key;
                                nextPointSelectionAction = null;
                                ShowCursor = false;
                                if (keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value == KeyDownStates.Down)
                                {
                                    keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value = KeyDownStates.Up; //Release magnifier if down but not locked down
                                }
                            }

                            //Reset and clean up
                            MagnifyAtPoint = null;
                            MagnifiedPointSelectionAction = null;
                        }, finalClickInSeries: false);
                        break;

                    case FunctionKeys.MouseLeftClick:
                        Log.Debug("Mouse left click selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseClickSoundFile, Settings.Default.MouseClickSoundVolume);
                                    outputService.LeftMouseButtonClick(fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseMiddleClick:
                        Log.Debug("Mouse middle click selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseClickSoundFile, Settings.Default.MouseClickSoundVolume);
                                    outputService.MiddleMouseButtonClick(fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;
                        
                    case FunctionKeys.MouseRightClick:
                        Log.Debug("Mouse right click selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseClickSoundFile, Settings.Default.MouseClickSoundVolume);
                                    outputService.RightMouseButtonClick(fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollAmountInClicks:
                        Log.Debug("Progressing MouseScrollAmountInClicks.");
                        switch (Settings.Default.MouseScrollAmountInClicks)
                        {
                            case 1:
                                Settings.Default.MouseScrollAmountInClicks = 3;
                                break;

                            case 3:
                                Settings.Default.MouseScrollAmountInClicks = 5;
                                break;

                            case 5:
                                Settings.Default.MouseScrollAmountInClicks = 10;
                                break;

                            case 10:
                                Settings.Default.MouseScrollAmountInClicks = 25;
                                break;

                            default:
                                Settings.Default.MouseScrollAmountInClicks = 1;
                                break;
                        }
                        break;

                    case FunctionKeys.MouseScrollToBottom:
                        Log.Debug("Mouse scroll to bottom selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelDown(Settings.Default.MouseScrollAmountInClicks, fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToBottomAndLeft:
                        Log.Debug("Mouse scroll to bottom and left selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelDownAndLeft(
                                        Settings.Default.MouseScrollAmountInClicks, fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToBottomAndRight:
                        Log.Debug("Mouse scroll to bottom and right selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelDownAndRight(
                                        Settings.Default.MouseScrollAmountInClicks, fp);
                                };

                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToLeft:
                        Log.Debug("Mouse scroll to left selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelLeft(Settings.Default.MouseScrollAmountInClicks, fp);
                                };
                                    
                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToRight:
                        Log.Debug("Mouse scroll to right selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelRight(Settings.Default.MouseScrollAmountInClicks, fp);
                                };
                                    
                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToTop:
                        Log.Debug("Mouse scroll to top selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelUp(Settings.Default.MouseScrollAmountInClicks, fp);
                                };
                                    
                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();  
                        });
                        break;

                    case FunctionKeys.MouseScrollToTopAndLeft:
                        Log.Debug("Mouse scroll to top and left selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelUpAndLeft(Settings.Default.MouseScrollAmountInClicks, fp);
                                };
                                    
                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MouseScrollToTopAndRight:
                        Log.Debug("Mouse scroll to top and right selected.");
                        SetupFinalClickAction(finalPoint =>
                        {
                            if (finalPoint != null)
                            {
                                Action<Point> simulateClick = fp =>
                                {
                                    audioService.PlaySound(Settings.Default.MouseScrollSoundFile, Settings.Default.MouseScrollSoundVolume);
                                    outputService.ScrollMouseWheelUpAndRight(Settings.Default.MouseScrollAmountInClicks, fp);
                                };
                                    
                                lastMouseActionStateManager.LastMouseAction = () => simulateClick(finalPoint.Value);
                                simulateClick(finalPoint.Value);
                            }

                            //Reset and clean up
                            ResetAndCleanupAfterMouseAction();
                        });
                        break;

                    case FunctionKeys.MoveAndResizeAdjustmentAmount:
                        Log.Debug("Progressing MoveAndResizeAdjustmentAmount.");
                        switch (Settings.Default.MoveAndResizeAdjustmentAmountInPixels)
                        {
                            case 1:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 5;
                                break;

                            case 5:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 10;
                                break;

                            case 10:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 25;
                                break;

                            case 25:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 50;
                                break;

                            case 50:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 100;
                                break;

                            default:
                                Settings.Default.MoveAndResizeAdjustmentAmountInPixels = 1;
                                break;
                        }
                        break;

                    case FunctionKeys.MoveToBottom:
                        Log.DebugFormat("Moving to bottom by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToBottom(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToBottomAndLeft:
                        Log.DebugFormat("Moving to bottom and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToBottomAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToBottomAndLeftBoundaries:
                        Log.Debug("Moving to bottom and left boundaries.");
                        mainWindowManipulationService.MoveToBottomAndLeftBoundaries();
                        break;
                        
                    case FunctionKeys.MoveToBottomAndRight:
                        Log.DebugFormat("Moving to bottom and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToBottomAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToBottomAndRightBoundaries:
                        Log.Debug("Moving to bottom and right boundaries.");
                        mainWindowManipulationService.MoveToBottomAndRightBoundaries();
                        break;
                        
                    case FunctionKeys.MoveToBottomBoundary:
                        Log.Debug("Moving to bottom boundary.");
                        mainWindowManipulationService.MoveToBottomBoundary();
                        break;
                        
                    case FunctionKeys.MoveToLeft:
                        Log.DebugFormat("Moving to left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToLeftBoundary:
                        Log.Debug("Moving to left boundary.");
                        mainWindowManipulationService.MoveToLeftBoundary();
                        break;
                        
                    case FunctionKeys.MoveToRight:
                        Log.DebugFormat("Moving to right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToRightBoundary:
                        Log.Debug("Moving to right boundary.");
                        mainWindowManipulationService.MoveToRightBoundary();
                        break;
                        
                    case FunctionKeys.MoveToTop:
                        Log.DebugFormat("Moving to top by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToTop(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToTopAndLeft:
                        Log.DebugFormat("Moving to top and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToTopAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToTopAndLeftBoundaries:
                        Log.Debug("Moving to top and left boundaries.");
                        mainWindowManipulationService.MoveToTopAndLeftBoundaries();
                        break;
                        
                    case FunctionKeys.MoveToTopAndRight:
                        Log.DebugFormat("Moving to top and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.MoveToTopAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;
                        
                    case FunctionKeys.MoveToTopAndRightBoundaries:
                        Log.Debug("Moving to top and right boundaries.");
                        mainWindowManipulationService.MoveToTopAndRightBoundaries();
                        break;
                        
                    case FunctionKeys.MoveToTopBoundary:
                        Log.Debug("Moving to top boundary.");
                        mainWindowManipulationService.MoveToTopBoundary();
                        break;

                    case FunctionKeys.NextSuggestions:
                        Log.Debug("Incrementing suggestions page.");

                        if (suggestionService.Suggestions != null
                            && (suggestionService.Suggestions.Count > (suggestionService.SuggestionsPage + 1) * SuggestionService.SuggestionsPerPage))
                        {
                            suggestionService.SuggestionsPage++;
                        }
                        break;

                    case FunctionKeys.NoQuestionResult:
                        HandleYesNoQuestionResult(false);
                        break;

                    case FunctionKeys.NumericAndSymbols1Keyboard:
                        Log.Debug("Changing keyboard to NumericAndSymbols1.");
                        Keyboard = new NumericAndSymbols1();
                        break;

                    case FunctionKeys.NumericAndSymbols2Keyboard:
                        Log.Debug("Changing keyboard to NumericAndSymbols2.");
                        Keyboard = new NumericAndSymbols2();
                        break;

                    case FunctionKeys.NumericAndSymbols3Keyboard:
                        Log.Debug("Changing keyboard to Symbols3.");
                        Keyboard = new NumericAndSymbols3();
                        break;

                    case FunctionKeys.PhysicalKeysKeyboard:
                        Log.Debug("Changing keyboard to PhysicalKeys.");
                        Keyboard = new PhysicalKeys();
                        break;

                    case FunctionKeys.PositionKeyboard:
                        Log.Debug("Changing keyboard to Position.");
                        Keyboard = new Position(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.PreviousSuggestions:
                        Log.Debug("Decrementing suggestions page.");

                        if (suggestionService.SuggestionsPage > 0)
                        {
                            suggestionService.SuggestionsPage--;
                        }
                        break;

                    case FunctionKeys.RepeatLastMouseAction:
                        if (lastMouseActionStateManager.LastMouseAction != null)
                        {
                            lastMouseActionStateManager.LastMouseAction();
                        }
                        break;

                    case FunctionKeys.RestoreFromMaximised:
                        Log.Debug("Restoring from maximised.");
                        mainWindowManipulationService.RestoreFromMaximised();
                        break;

                    case FunctionKeys.RestoreFromMinimised:
                        Log.Debug("Restoring from minimised.");
                        var minimisedAsNavigableKeyboard = Keyboard as IBackAction;
                        if (minimisedAsNavigableKeyboard != null && minimisedAsNavigableKeyboard.BackAction != null)
                        {
                            minimisedAsNavigableKeyboard.BackAction();
                        }
                        else
                        {
                            Keyboard = new Alpha();
                        }
                        mainWindowManipulationService.RestoreFromMinimised();
                        break;

                    case FunctionKeys.ShrinkFromBottom:
                        Log.DebugFormat("Shrinking from bottom by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromBottom(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromBottomAndLeft:
                        Log.DebugFormat("Shrinking from bottom and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromBottomAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromBottomAndRight:
                        Log.DebugFormat("Shrinking from bottom and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromBottomAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromLeft:
                        Log.DebugFormat("Shrinking from left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromRight:
                        Log.DebugFormat("Shrinking from right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromTop:
                        Log.DebugFormat("Shrinking from top by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromTop(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromTopAndLeft:
                        Log.DebugFormat("Shrinking from top and left by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromTopAndLeft(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.ShrinkFromTopAndRight:
                        Log.DebugFormat("Shrinking from top and right by {0}px.", Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        mainWindowManipulationService.ShrinkFromTopAndRight(Settings.Default.MoveAndResizeAdjustmentAmountInPixels);
                        break;

                    case FunctionKeys.SizeKeyboard:
                        Log.Debug("Changing keyboard to Size.");
                        Keyboard = new Size(() => Keyboard = currentKeyboard);
                        break;

                    case FunctionKeys.Speak:
                        audioService.Speak(
                            outputService.Text,
                            Settings.Default.SpeechVolume,
                            Settings.Default.SpeechRate,
                            Settings.Default.SpeechVoice);
                        break;

                    case FunctionKeys.YesQuestionResult:
                        HandleYesNoQuestionResult(true);
                        break;
                }

                outputService.ProcessFunctionKey(singleKeyValue.FunctionKey.Value);
            }
        }

        private void SetupFinalClickAction(Action<Point?> finalClickAction, bool finalClickInSeries = true)
        {
            nextPointSelectionAction = nextPoint =>
            {
                if (keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value.IsDownOrLockedDown())
                {
                    ShowCursor = false; //Ensure cursor is not showing when MagnifyAtPoint is set because...
                    //1.This triggers a screen capture, which shouldn't have the cursor in it.
                    //2.Last popup open stays on top (I know the VM in MVVM shouldn't care about this, so pretend it's all reason 1).
                    MagnifiedPointSelectionAction = finalClickAction;
                    MagnifyAtPoint = nextPoint;
                    ShowCursor = true;
                }
                else
                {
                    finalClickAction(nextPoint);
                }

                if (finalClickInSeries)
                {
                    nextPointSelectionAction = null;
                }
            };

            SelectionMode = SelectionModes.Point;
            ShowCursor = true;
        }

        private void ResetAndCleanupAfterMouseAction()
        {
            SelectionMode = SelectionModes.Key;
            nextPointSelectionAction = null;
            ShowCursor = false;
            MagnifyAtPoint = null;
            MagnifiedPointSelectionAction = null;
            if (keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value == KeyDownStates.Down)
            {
                keyboardService.KeyDownStates[KeyValues.MouseMagnifierKey].Value = KeyDownStates.Up; //Release magnifier if down but not locked down
            }
        }

        public void HandleServiceError(object sender, Exception exception)
        {
            Log.Error("Error event received from service. Raising ErrorNotificationRequest and playing ErrorSoundFile (from settings)", exception);

            inputService.RequestSuspend();
            audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
            RaiseToastNotification("Uh-oh!", exception.Message, NotificationTypes.Error, () => { inputService.RequestResume(); });
        }
    }
}
