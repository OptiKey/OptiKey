using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
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
