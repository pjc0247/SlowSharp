using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class HybType
    {
        public bool isCompiledType => compiledType != null;

        public Type compiledType { get; }
        public Class interpretKlass { get; }

        public static HybType Object => HybTypeCache.Object;
        public static HybType Bool => HybTypeCache.Bool;
        public static HybType Int => HybTypeCache.Int;
        public static HybType String => HybTypeCache.String;
        public static HybType Float => HybTypeCache.Float;
        public static HybType Double => HybTypeCache.Double;
        public static HybType Decimal => HybTypeCache.Decimal;

        public HybType(Type type)
        {
            this.compiledType = type;
        }
        public HybType(Class klass)
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
                    Activator.CreateInstance(compiledType, args));
            }
            else
            {
                var inst = new HybInstance(runner, this, interpretKlass);
                var ctor = inst.GetMethods("$_ctor");

                if (ctor.Length > 0)
                    ctor[0].Invoke(inst, args);

                return inst;
            }
        }

        public Invokable[] GetMethods(string id)
        {
            if (isCompiledType)
            {
                return compiledType.GetMethods()
                   .Where(x => x.Name == id)
                   .Select(x => new Invokable(x))
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
    }
}
