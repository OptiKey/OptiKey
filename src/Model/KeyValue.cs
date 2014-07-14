using System.Text;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Model
{
    public struct KeyValue
    {
        public FunctionKeys? FunctionKey { get; set; }
        public char? Char { get; set; }
        public string String { get; set; }

        public char? Letter
        {
            get { return Char != null && char.IsLetter(Char.Value) ? Char.Value : (char?)null; }
        }
        
        public bool Equals(KeyValue kv)
        {
            // Return true if the fields match:
            return (FunctionKey == kv.FunctionKey)
                && (Char == kv.Char)
                && (String == kv.String);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (FunctionKey != null)
            {
                stringBuilder.Append(FunctionKey);
            }

            if (Char != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }
                stringBuilder.Append(Char);
            }

            if (String != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }
                stringBuilder.Append(String);
            }
            
            return stringBuilder.ToString();
        }
    }
}
