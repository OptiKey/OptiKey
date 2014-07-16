using System.Text;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Model
{
    public struct KeyValue
    {
        public FunctionKeys? FunctionKey { get; set; }
        public string String { get; set; }

        public bool StringIsLetter
        {
            get { return String != null && String.Length == 1 && char.IsLetter(String, 0); }
        }
        
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
                stringBuilder.Append(String);
            }
            
            return stringBuilder.ToString();
        }
    }
}
