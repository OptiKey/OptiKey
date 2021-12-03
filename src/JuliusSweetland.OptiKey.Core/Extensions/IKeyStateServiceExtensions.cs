// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using log4net;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class IKeyStateServiceExtensions
    {
        public static Action ReleaseModifiers(this IKeyStateService keyStateService, ILog log)
        {
            var lastLeftShiftValue = keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value;
            var lastLeftCtrlValue = keyStateService.KeyDownStates[KeyValues.LeftCtrlKey].Value;
            var lastLeftWinValue = keyStateService.KeyDownStates[KeyValues.LeftWinKey].Value;
            var lastLeftAltValue = keyStateService.KeyDownStates[KeyValues.LeftAltKey].Value;
            
            log.InfoFormat("Releasing modifiers (shift:{0}, ctrl:{1}, win:{2}, alt:{3})", 
                lastLeftShiftValue, lastLeftCtrlValue, lastLeftWinValue, lastLeftAltValue);
            
            keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Up;
            keyStateService.KeyDownStates[KeyValues.LeftCtrlKey].Value = KeyDownStates.Up;
            keyStateService.KeyDownStates[KeyValues.LeftWinKey].Value = KeyDownStates.Up;
            keyStateService.KeyDownStates[KeyValues.LeftAltKey].Value = KeyDownStates.Up;
            return () =>
            {
                log.InfoFormat("Restoring modifiers (shift:{0}, ctrl:{1}, win:{2}, alt:{3})",
                    lastLeftShiftValue, lastLeftCtrlValue, lastLeftWinValue, lastLeftAltValue);
                keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = lastLeftShiftValue;
                keyStateService.KeyDownStates[KeyValues.LeftCtrlKey].Value = lastLeftCtrlValue;
                keyStateService.KeyDownStates[KeyValues.LeftWinKey].Value = lastLeftWinValue;
                keyStateService.KeyDownStates[KeyValues.LeftAltKey].Value = lastLeftAltValue;
            };
        }
    }
}
