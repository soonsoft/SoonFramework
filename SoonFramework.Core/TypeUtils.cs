using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class TypeUtils
    {
        public static bool IsNullableType(Type type)
        {
            Guard.ArgumentNotNull(type, "type");

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
