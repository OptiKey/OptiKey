using System;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyValueAndTimeSpan : BindableBase, IEquatable<KeyValueAndTimeSpan>
    {
        private readonly string name;
        private readonly KeyValue keyValue;
        private TimeSpan? timeSpan;

        public KeyValueAndTimeSpan(string name, KeyValue keyValue, TimeSpan? timeSpan)
        {
            this.name = name;
            this.keyValue = keyValue;
            this.timeSpan = timeSpan;
        }

        public string Name { get { return name; } }
        public KeyValue KeyValue { get { return keyValue; } }
        public double? TimeSpanTotalMilliseconds
        {
            get { return timeSpan != null ? timeSpan.Value.TotalMilliseconds : (double?)null; }
            set { SetProperty(ref timeSpan, value != null ? TimeSpan.FromMilliseconds(value.Value) : (TimeSpan?)null); }
        }

        #region IEquatable

        public bool Equals(KeyValueAndTimeSpan kvats)
        {
            if (ReferenceEquals(kvats, null)) return false;

            return (Name == kvats.Name)
                && (KeyValue == kvats.KeyValue)
                && (TimeSpanTotalMilliseconds == kvats.TimeSpanTotalMilliseconds);
        }

        public static bool operator ==(KeyValueAndTimeSpan kv1, KeyValueAndTimeSpan kv2)
        {
            if (ReferenceEquals(kv1, null))
            {
                return ReferenceEquals(kv2, null);
            }
            return kv1.Equals(kv2);
        }

        public static bool operator !=(KeyValueAndTimeSpan x, KeyValueAndTimeSpan y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
           return Equals(obj as KeyValueAndTimeSpan);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 397) ^ KeyValue.GetHashCode(); //Struct so not nullable
                hash = (hash * 397) ^ (TimeSpanTotalMilliseconds != null ? TimeSpanTotalMilliseconds.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion
    }
}
