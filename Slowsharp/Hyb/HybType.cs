using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class HybType
    {
        public bool isCompiledType => compiledType != null;

        internal Type compiledType { get; }
        internal Class interpretKlass { get; }

        public static HybType Object => HybTypeCache.Object;
        public static HybType Bool => HybTypeCache.Bool;
        public static HybType Int16 => HybTypeCache.Int16;
        public static HybType Int32 => HybTypeCache.Int;
        public static HybType Int64 => HybTypeCache.Int64;
        public static HybType Short => HybTypeCache.Short;
        public static HybType Char => HybTypeCache.Char;
        public static HybType String => HybTypeCache.String;
        public static HybType Float => HybTypeCache.Float;
        public static HybType Double => HybTypeCache.Double;
        public static HybType Decimal => HybTypeCache.Decimal;

        internal HybType(Type type)
        {
            this.compiledType = type;
        }
        internal HybType(Class klass)
        {
            this.interpretKlass = klass;
        }

        public HybType MakeGenericType(HybType[] genericArgs)
        {
            if (isCompiledType)
            {
                return new HybType(compiledType.MakeGenericType(genericArgs.Unwrap()));
            }
            return null;
        }

        public HybInstance CreateInstance(Runner runner, HybInstance[] args)
        {
            if (isCompiledType)
            {
                return new HybInstance(
                    this,
                    Activator.CreateInstance(compiledType, args.Unwrap()));
            }
            else
            {
                var inst = new HybInstance(runner, this, interpretKlass);
                var ctor = inst.GetMethods("$_ctor");

                if (ctor.Length > 0)
                    ctor[0].target.Invoke(inst, args);

                return inst;
            }
        }

        public SSMethodInfo[] GetStaticMethods(string id)
        {
            if (isCompiledType)
            {
                return compiledType.GetMethods()
                   .Where(x => x.IsStatic)
                   .Where(x => x.Name == id)
                   .Select(x => new SSMethodInfo(x) {
                       id = x.Name,
                       isStatic = x.IsStatic,
                       accessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                return interpretKlass.GetMethods(id);
            }
        }

        public HybInstance GetDefault()
        {
            if (isCompiledType)
            {
                if (compiledType.IsValueType)
                    return HybInstance.Object(Activator.CreateInstance(compiledType));
                return HybInstance.Null();
            }
            return HybInstance.Null();
        }

        public override string ToString()
        {
            if (isCompiledType)
                return compiledType.Name;
            return interpretKlass.id;
        }
    }
}
