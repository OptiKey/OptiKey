using System;
using WindowsInput;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public class PublishService : IPublishService
    {
        private readonly InputSimulator inputSimulator;

        public PublishService()
        {
            inputSimulator = new InputSimulator();
        }

        public void SynchroniseKeyboardState()
        {
            throw new System.NotImplementedException();
        }

        #region KeyDown

        public void KeyDown(FunctionKeys? functionKey, char? character)
        {
            if (functionKey != null)
            {
                var virtualKeyCodeSet = functionKey.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    KeyDown(virtualKeyCodeSet.Value);
                }
            }

            if (character != null)
            {
                var virtualKeyCodeSet = character.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    KeyDown(virtualKeyCodeSet.Value);
                }
            }
        }

        private void KeyDown(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            if (virtualKeyCodeSet.ModifierKeyCodes != null)
            {
                foreach (var modifierKeyCode in virtualKeyCodeSet.ModifierKeyCodes)
                {
                    inputSimulator.Keyboard.KeyDown(modifierKeyCode);
                }
            }

            if (virtualKeyCodeSet.KeyCodes != null)
            {
                foreach (var keyCode in virtualKeyCodeSet.KeyCodes)
                {
                    inputSimulator.Keyboard.KeyDown(keyCode);
                }
            }
        }

        #endregion

        #region KeyUp

        public void KeyUp(FunctionKeys? functionKey, char? character)
        {
            if (functionKey != null)
            {
                var virtualKeyCodeSet = functionKey.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    KeyUp(virtualKeyCodeSet.Value);
                }
            }

            if (character != null)
            {
                var virtualKeyCodeSet = character.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    KeyUp(virtualKeyCodeSet.Value);
                }
            }
        }

        private void KeyUp(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            if (virtualKeyCodeSet.ModifierKeyCodes != null)
            {
                foreach (var modifierKeyCode in virtualKeyCodeSet.ModifierKeyCodes)
                {
                    inputSimulator.Keyboard.KeyUp(modifierKeyCode);
                }
            }

            if (virtualKeyCodeSet.KeyCodes != null)
            {
                foreach (var keyCode in virtualKeyCodeSet.KeyCodes)
                {
                    inputSimulator.Keyboard.KeyUp(keyCode);
                }
            }
        }

        #endregion

        public void KeyPress(FunctionKeys? functionKey, char? character)
        {
            if (functionKey != null)
            {
                var virtualKeyCodeSet = functionKey.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    inputSimulator.Keyboard.ModifiedKeyStroke(
                        virtualKeyCodeSet.Value.ModifierKeyCodes,
                        virtualKeyCodeSet.Value.KeyCodes);
                }
            }

            if (character != null)
            {
                var virtualKeyCodeSet = character.Value.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    inputSimulator.Keyboard.ModifiedKeyStroke(
                        virtualKeyCodeSet.Value.ModifierKeyCodes,
                        virtualKeyCodeSet.Value.KeyCodes);
                }
                else
                {
                    inputSimulator.Keyboard.TextEntry(character.Value);
                }
            }
        }
    }
}
