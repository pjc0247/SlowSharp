using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal static class HybExt
    {
        public static object Unwrap(this HybInstance _this)
        {
            return _this.innerObject;
        }
        public static Type[] Unwrap(this HybType[] _this)
        {
            var objs = new Type[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = _this[i].compiledType;

            return objs;
        }
        public static object[] Unwrap(this HybInstance[] _this)
        {
            var objs = new object[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = _this[i].innerObject;

            return objs;
        }
        public static HybInstance[] Wrap(this object[] _this)
        {
            var objs = new HybInstance[_this.Length];

            for (int i = 0; i < _this.Length; i++)
                objs[i] = HybInstance.Object(_this[i]);

            return objs;
        }
        public static HybInstance Wrap(this object _this)
        {
            if (_this is HybInstance hyb) return hyb;
            return HybInstance.Object(_this);
        }

        public static bool IsSubclassOf(this Type _this, HybType type)
        {
            if (type.isCompiledType == false)
                return false;

            return _this.IsSubclassOf(type.compiledType);
        }
    }
}
