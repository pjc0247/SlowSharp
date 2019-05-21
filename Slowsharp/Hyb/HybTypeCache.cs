using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class HybTypeCache
    {
        public static readonly HybType Void = new HybType(typeof(void));
        public static readonly HybType Object = new HybType(typeof(object));
        public static readonly HybType Bool = new HybType(typeof(bool));
        public static readonly HybType Int = new HybType(typeof(int));
        public static readonly HybType Int16 = new HybType(typeof(Int16));
        public static readonly HybType Int64 = new HybType(typeof(Int64));
        public static readonly HybType Uint = new HybType(typeof(uint));
        public static readonly HybType Uint16 = new HybType(typeof(UInt16));
        public static readonly HybType Uint64 = new HybType(typeof(UInt64));
        public static readonly HybType Short = new HybType(typeof(short));
        public static readonly HybType Ushort = new HybType(typeof(ushort));
        public static readonly HybType String = new HybType(typeof(string));
        public static readonly HybType Char = new HybType(typeof(char));
        public static readonly HybType Byte = new HybType(typeof(byte));
        public static readonly HybType Sbyte = new HybType(typeof(sbyte));
        public static readonly HybType Float = new HybType(typeof(float));
        public static readonly HybType Double = new HybType(typeof(double));
        public static readonly HybType Decimal = new HybType(typeof(decimal));
        public static readonly HybType Type = new HybType(typeof(Type));

        private static ThreadLocal<Dictionary<Type, HybType>> CompiledTypes = new ThreadLocal<Dictionary<Type, HybType>>(() =>
        {
            return new Dictionary<Type, HybType>();
        });

        static HybTypeCache()
        {

        }

        public static HybType GetHybType(Type type)
        {
            var cache = CompiledTypes.Value;
            HybType result = null;

            if (cache.TryGetValue(type, out result))
                return result;

            result = new HybType(type);
            cache[type] = result;
            return result;
        }
    }
}
