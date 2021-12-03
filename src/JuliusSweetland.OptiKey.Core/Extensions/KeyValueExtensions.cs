// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Text;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Extensions
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
    }
}
