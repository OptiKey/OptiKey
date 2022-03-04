// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        ///     Swaps two elements in an array. No bounds checking is done.
        /// </summary>
        /// <param name="list">The list to swap elements in.</param>
        /// <param name="first">The index of the element to swap to the second index position.</param>
        /// <param name="second">The index of the element to swap to the first index position.</param>
        public static void Swap(this IList list, int first, int second)
        {
            var temp = list[first];
            list[first] = list[second];
            list[second] = temp;
        }
    }
}