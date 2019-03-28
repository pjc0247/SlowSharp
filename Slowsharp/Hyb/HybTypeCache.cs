using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    class HybTypeCache
    {
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
        public static readonly HybType Float = new HybType(typeof(float));
        public static readonly HybType Double = new HybType(typeof(double));
        public static readonly HybType Decimal = new HybType(typeof(decimal));

        static HybTypeCache()
        {

        }
    }
}
