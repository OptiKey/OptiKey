// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Models
{
    [TypeConverter(typeof(KeyValueConverter))]
    public class KeyValue : IEquatable<KeyValue>
    {

        private readonly FunctionKeys? functionKey;
        private readonly string str;

        public KeyValue()
        {
            this.functionKey = null;
            this.str = null;
        }

        public KeyValue(FunctionKeys functionKey)
        {
            this.functionKey = functionKey;
            this.str = null;
        }

        public KeyValue(string str)
        {
            this.functionKey = null;
            this.str = str;
        }

        public KeyValue(FunctionKeys? functionKey, string str)
        {
            this.functionKey = functionKey;
            this.str = str;
        }

        public FunctionKeys? FunctionKey { get { return functionKey; } }
        public string String { get { return str; } }

        public List<KeyCommand> Commands { get; set; }

        public bool StringIsLetter
        {
            get { return String != null && String.Length == 1 && char.IsLetter(String, 0); }
        }
        
        #region IEquatable

        public static bool Equals(KeyValue x, KeyValue y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyValue return false:
            KeyValue p = obj as KeyValue;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(KeyValue kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(KeyValue x, KeyValue y)
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

            // Return true if the fields and hash codes match
            // Hash code check allows us to account for differences in 
            // subclasses, even if the (KeyValue) parts are equal.
            return (x.FunctionKey == y.FunctionKey)
                && (x.String == y.String)
                && x.GetHashCode() == y.GetHashCode();
        }

        public static bool operator !=(KeyValue x, KeyValue y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 397) ^ (FunctionKey != null ? FunctionKey.GetHashCode() : 0);
                hash = (hash * 397) ^ (String != null ? String.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (FunctionKey != null)
            {
                stringBuilder.Append(FunctionKey);
            }

            if (String != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }

                //Special chars such as '\n' have meaning in a string - convert to literal strings.
                //This is also required by the Key property as Key is used in dictionary indexes, for example.
                stringBuilder.Append(String.ToPrintableString());
            }
            
            return stringBuilder.ToString();
        }

        public virtual bool HasContent() {            
            return (FunctionKey != null) ||
                   (!string.IsNullOrEmpty(String)); 
        }
    }
}
