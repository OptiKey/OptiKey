using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.ETTA.Model;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class KeyValueExtensions
    {
        public static string ReduceToSequentiallyDistinctLetters(
            this List<KeyValue> keyValues, 
            int minSequenceLength,
            Char? prefix,
            Char? suffix)
        {
            var output = new List<Char>();

            for (int index = 0; index < keyValues.Count; index++)
            {
                Char? currentLetter = keyValues[index].Letter;

                if (currentLetter != null)
                {
                    var charCount = 1;
                    index++;

                    while (index < keyValues.Count
                            && keyValues[index].Char != null
                            && keyValues[index].Char.Value == currentLetter)
                    {
                        charCount++;
                        index++;
                    }

                    //Only add key to output if there is an acceptably long string of the same letter
                    if (charCount >= minSequenceLength)
                    {
                        //Add to output if not the same as previously output character
                        if(!output.Any() || !output[output.Count - 1].Equals(currentLetter.Value))
                        {
                            output.Add(currentLetter.Value);
                        }
                    }
                }

                index--;
            }

            //Add prefix
            if (prefix != null
                && output.Any()
                && output.First() != prefix.Value)
            {
                output.Insert(0, prefix.Value);
            }

            //Add suffix
            if (suffix != null
                && output.Any()
                && output.Last() != suffix.Value)
            {
                output.Add(suffix.Value);
            }

            return output.Any() ? new string(output.ToArray()) : null;
        }
    }
}
