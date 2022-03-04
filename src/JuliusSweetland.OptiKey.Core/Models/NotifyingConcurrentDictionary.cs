// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Concurrent;
using System.Collections.Generic;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotifyingConcurrentDictionary<TKey, TValue> : BindableBase
    {
        private readonly ConcurrentDictionary<TKey, NotifyingProxy<TValue>> dictionary =
            new ConcurrentDictionary<TKey, NotifyingProxy<TValue>>();

        public void Clear()
        {
            foreach (TKey key in dictionary.Keys)
            {
                dictionary[key].Value = default(TValue);
            }
        }

        public NotifyingProxy<TValue> this[TKey key]
        {
            get
            {
                return dictionary.GetOrAdd(key, new NotifyingProxy<TValue>(default(TValue)));
            }
            set
            {
                dictionary.AddOrUpdate(key, value, (k, existingValue) =>
                {
                    existingValue.Value = value.Value;    
                    return existingValue;
                });
            }
        }

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }
    }
}
