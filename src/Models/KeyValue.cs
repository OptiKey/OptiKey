using System.Text;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;

namespace JuliusSweetland.ETTA.Models
{
    public struct KeyValue
    {
        public FunctionKeys? FunctionKey { get; set; }
        public string String { get; set; }

        public bool StringIsLetter
        {
            get { return String != null && String.Length == 1 && char.IsLetter(String, 0); }
        }

        /// <summary>
        /// Unique key to be used in dictionaries etc
        /// </summary>
        public string Key { get { return this.ToString(); } }
        
        public bool Equals(KeyValue kv)
        {
            // Return true if the fields match:
            return (FunctionKey == kv.FunctionKey)
                && (String == kv.String);
        }

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
                stringBuilder.Append(String.ConvertEscapedCharsToLiterals());
            }
            
            return stringBuilder.ToString();
        }
    }
}
