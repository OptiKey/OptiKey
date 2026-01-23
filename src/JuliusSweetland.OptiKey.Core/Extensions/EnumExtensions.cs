using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsObsolete(this Enum value)
        {
            var enumType = value.GetType();
            var enumName = enumType.GetEnumName(value);
            var fieldInfo = enumType.GetField(enumName);
            return Attribute.IsDefined(fieldInfo, typeof(ObsoleteAttribute));
        }
    }
}
