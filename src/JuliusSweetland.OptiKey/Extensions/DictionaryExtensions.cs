using System;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merge keys present inside two or more dictionaries.
        /// In case of duplicates, "righter" dictionaries values will erase "lefter" ones.
        /// Keeps the type of 'this', which must be default-instantiable.
        /// Example: 
        ///   map.MergeLeft(other1, other2, ...)
        ///   
        /// Inspired from http://stackoverflow.com/a/2679857/1182976
        /// </summary>
        /// <param name="me">Left dictionary merged. Modified.</param>
        /// <param name="others">Other right dictionaries to merge into "me". Not modified.</param>
        /// <returns>Merge result, which is me instance</returns>
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            foreach (IDictionary<K, V> src in others)
            {
                foreach (KeyValuePair<K, V> p in src)
                {
                    me[p.Key] = p.Value;
                }
            }
            return me;
        }

    }
}
