// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyPressKeyValue : KeyValue, IEquatable<KeyPressKeyValue>
    {
        
        public enum KeyPressType { PressAndRelease, Press, Release };

        private readonly string key; // letter, e.g. 'a', or string for function key e.g.'F2'
        private readonly int durationMs;
        private readonly KeyPressType type;

        public KeyPressKeyValue() : base()
        {
            this.key = null;
            this.type = KeyPressType.PressAndRelease;
            this.durationMs = 0; 
        }

        public KeyPressKeyValue(string key, KeyPressType type = default(KeyPressType), int duration = 0)
            : base()
        {
            this.key = key;
            this.type = type;
            this.durationMs = duration;
        }

        public override bool HasContent()
        {
            return this.key != null;
        }

        public KeyPressType Type { get { return type; } }

        public int DurationMs { get { return durationMs; } }

        // Use FunctionKeysExtensions to convert to something more usable
        public string Key { get { return key; } }


        #region IEquatable

        public static bool Equals(KeyPressKeyValue x, KeyPressKeyValue y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyPressKeyValue return false:
            KeyPressKeyValue p = obj as KeyPressKeyValue;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(KeyPressKeyValue kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(KeyPressKeyValue x, KeyPressKeyValue y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (x.Key == y.Key) &&
                   (x.Type == y.Type) &&
                   (x.DurationMs == y.DurationMs);
        }

        public static bool operator !=(KeyPressKeyValue x, KeyPressKeyValue y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 127) ^ (Key != null ? Key.GetHashCode() : 0);
                hash = (hash * 127) ^ (Type.GetHashCode());
                hash = (hash * 127) ^ (DurationMs.GetHashCode());
                return hash;
            }
        }

        #endregion

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(Key == null ? "null key" : Key);
            stringBuilder.Append(",");

            stringBuilder.Append(Type.ToString());
            stringBuilder.Append(",");

            stringBuilder.Append(DurationMs);

            return stringBuilder.ToString();
        }
    }
}
