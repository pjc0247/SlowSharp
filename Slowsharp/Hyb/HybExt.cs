using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class HybExt
    {
        public static bool IsSubclassOf(this Type _this, HybType type)
        {
            if (type.isCompiledType == false)
                return false;

            return _this.IsSubclassOf(type.compiledType);
        }
    }
}
