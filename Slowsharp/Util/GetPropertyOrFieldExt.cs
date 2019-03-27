using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class GetPropertyOrFieldExt
    {
        public static object GetValue(this MemberInfo _this, object obj)
        {
            if (_this is PropertyInfo pi)
                return pi.GetValue(obj);
            if (_this is FieldInfo fi)
                return fi.GetValue(obj);
            return null;
        }
    }
}
