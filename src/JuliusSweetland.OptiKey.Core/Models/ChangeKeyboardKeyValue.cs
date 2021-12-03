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
    public class ChangeKeyboardKeyValue : KeyValue, IEquatable<ChangeKeyboardKeyValue>
    {

        // Payload is either a builtin keyboard (by enum) or a string
        // defining file path for XML file. 
        private readonly Enums.Keyboards? builtinKeyboard;
        private readonly string keyboardFilename;

        // replace current keyboard instead of pushing on top?
        private readonly bool replaceKeyboard;

        public ChangeKeyboardKeyValue() : base()
        {
            this.keyboardFilename = null;
        }

        public ChangeKeyboardKeyValue(Enums.Keyboards keyboardEnum, bool replacePreviousKeyboard = false)
        {
            this.builtinKeyboard = keyboardEnum;
            this.replaceKeyboard = replacePreviousKeyboard;
        }

        public ChangeKeyboardKeyValue(string keyboardLink, bool replacePreviousKeyboard=false)
        {
            this.keyboardFilename = keyboardLink;
            this.replaceKeyboard = replacePreviousKeyboard;
        }

        public override bool HasContent()
        {
            return (this.keyboardFilename != null ||
                    this.builtinKeyboard.HasValue);
        }

        public string KeyboardFilename { get { return keyboardFilename; } }
        public Enums.Keyboards? BuiltInKeyboard { get { return builtinKeyboard; } }
        public bool Replace { get { return replaceKeyboard; } }

        #region IEquatable

        public static bool Equals(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyValueLink return false:
            ChangeKeyboardKeyValue p = obj as ChangeKeyboardKeyValue;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(ChangeKeyboardKeyValue kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
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
            return (x.BuiltInKeyboard == y.BuiltInKeyboard) &&
                   (x.KeyboardFilename == y.KeyboardFilename) &&
                   (x.Replace == y.Replace);
        }

        public static bool operator !=(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 389) ^ (KeyboardFilename != null ? KeyboardFilename.GetHashCode() : 0);
                hash = (hash * 7) ^ BuiltInKeyboard.GetHashCode();
                hash = (hash * 13) ^ Replace.GetHashCode();
                return hash;
            }
        }

        #endregion

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (KeyboardFilename != null)
            {
                stringBuilder.Append(KeyboardFilename);
            }
            if (BuiltInKeyboard.HasValue)
            {
                stringBuilder.Append(BuiltInKeyboard);
            }

            stringBuilder.Append(" replace = ");
            stringBuilder.Append(this.replaceKeyboard);
            
            return stringBuilder.ToString();
        }
    }
}
