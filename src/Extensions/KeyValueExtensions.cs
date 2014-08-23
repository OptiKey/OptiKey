using System;
using System.Collections.Generic;
using System.Text;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class KeyValueExtensions
    {
        public static string ReduceToSequentiallyDistinctLetters(
            this List<KeyValue> keyValues, 
            int minSequenceLength,
            string prefixLetter,
            string suffixLetter)
        {
            var output = new StringBuilder();

            for (int index = 0; index < keyValues.Count; index++)
            {
                string currentLetter = keyValues[index].StringIsLetter ? keyValues[index].String : null;

                if (currentLetter != null)
                {
                    var charCount = 1;
                    index++;

                    while (index < keyValues.Count
                        && string.Equals(keyValues[index].String, currentLetter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        charCount++;
                        index++;
                    }

                    //Only add key to output if there is an acceptably long string of the same letter
                    if (charCount >= minSequenceLength)
                    {
                        //Add to output if not the same as previously output character
                        if(output.Length == 0 || !output[output.Length - 1].Equals(currentLetter[0]))
                        {
                            output.Append(currentLetter);
                        }
                    }

                    index--;
                }
            }

            //Add prefixLetter
            if (prefixLetter != null
                && (output.Length == 0 || output[0] != prefixLetter[0]))
            {
                output.Insert(0, prefixLetter[0]);
            }

            //Add suffixLetter
            if (suffixLetter != null
                && (output.Length == 0 || output[output.Length - 1] != suffixLetter[0]))
            {
                output.Append(suffixLetter[0]);
            }

            return output.Length > 0 ? output.ToString() : null;
        }

        public static bool IsPublishOnly(this KeyValue kv)
        {
            switch (kv.FunctionKey)
            {
                case FunctionKeys.Tab:
                case FunctionKeys.Ctrl:
                case FunctionKeys.Win:
                case FunctionKeys.Alt:
                case FunctionKeys.F1:
                case FunctionKeys.F2:
                case FunctionKeys.F3:
                case FunctionKeys.F4:
                case FunctionKeys.F5:
                case FunctionKeys.F6:
                case FunctionKeys.F7:
                case FunctionKeys.F8:
                case FunctionKeys.F9:
                case FunctionKeys.F10:
                case FunctionKeys.F11:
                case FunctionKeys.F12:
                case FunctionKeys.PrintScreen:
                case FunctionKeys.ScrollLock:
                case FunctionKeys.NumberLock:
                case FunctionKeys.Menu:
                case FunctionKeys.ArrowUp:
                case FunctionKeys.ArrowLeft:
                case FunctionKeys.ArrowRight:
                case FunctionKeys.ArrowDown:
                case FunctionKeys.Break:
                case FunctionKeys.Insert:
                case FunctionKeys.Home:
                case FunctionKeys.PgUp:
                case FunctionKeys.PgDn:
                case FunctionKeys.Delete:
                case FunctionKeys.End:
                case FunctionKeys.Escape:
                case FunctionKeys.SelectAll:
                case FunctionKeys.Cut:
                case FunctionKeys.Copy:
                case FunctionKeys.Paste:
                    return true;
            }

            return false;
        }
    }
}
