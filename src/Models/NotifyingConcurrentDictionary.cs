using System.Collections.Concurrent;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class NotifyingConcurrentDictionary<T> : BindableBase
    {
        private readonly ConcurrentDictionary<string, NotifyingProxy<T>> dictionary =
            new ConcurrentDictionary<string, NotifyingProxy<T>>();

        public void Clear()
        {
            dictionary.Clear();
            OnPropertyChanged(() => this);
        }

        public NotifyingProxy<T> this[string key]
        {
            get
            {
                return dictionary.GetOrAdd(key, new NotifyingProxy<T>(default(T)));
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
    }
}
