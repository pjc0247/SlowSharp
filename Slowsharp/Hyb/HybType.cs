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
        public static HybType Void => HybTypeCache.Void;
        public static HybType Object => HybTypeCache.Object;
        public static HybType Bool => HybTypeCache.Bool;
        public static HybType Int16 => HybTypeCache.Int16;
        public static HybType Int32 => HybTypeCache.Int;
        public static HybType Int64 => HybTypeCache.Int64;
        public static HybType Uint16 => HybTypeCache.Uint16;
        public static HybType Uint32 => HybTypeCache.Uint;
        public static HybType Uint64 => HybTypeCache.Uint64;
        public static HybType Short => HybTypeCache.Short;
        public static HybType Ushort => HybTypeCache.Ushort;
        public static HybType Char => HybTypeCache.Char;
        public static HybType Byte => HybTypeCache.Byte;
        public static HybType Sbyte => HybTypeCache.Sbyte;
        public static HybType String => HybTypeCache.String;
        public static HybType Float => HybTypeCache.Float;
        public static HybType Double => HybTypeCache.Double;
        public static HybType Decimal => HybTypeCache.Decimal;

        public bool isCompiledType => compiledType != null;
        public bool isInterface { get; }
        public bool isSealed { get; }

        public bool isArray { get; }
        public int arrayRank { get; }
        public HybType elementType { get; }

        internal Type compiledType { get; }
        internal Class interpretKlass { get; }

        internal HybType(Type type)
        {
            this.isSealed = type.IsSealed;
            this.isInterface = type.IsInterface;
            this.compiledType = type;
        }
        internal HybType(Class klass)
        {
            this.isSealed = false;
            this.isInterface = false;
            this.interpretKlass = klass;
        }
        internal HybType(Class klass, HybType elementType, int arrayRank) :
            this(klass)
        {
            this.isArray = true;
            this.elementType = elementType;
            this.arrayRank = arrayRank;
        }

        public HybType MakeArrayType(int rank)
        {
            if (isCompiledType)
            {
                if (rank == 1)
                    return new HybType(compiledType.MakeArrayType());
                return new HybType(compiledType.MakeArrayType(rank));
            }
            return new HybType(interpretKlass, this, arrayRank);
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
            // Array with interpret type
            else if (isArray)
            {
                var inst = Array.CreateInstance(typeof(HybInstance),
                    args.Unwrap().Select(x => (int)x).ToArray());
                return HybInstance.Object(inst);
            }
            // Interpret type object
            else
            {
                var inst = new HybInstance(runner, this, interpretKlass);
                var ctors = inst.GetMethods("$_ctor");

                if (ctors.Length > 0)
                {
                    var ctor = OverloadingResolver
                        .FindMethodWithArguments(runner.resolver, ctors, args);
                    ctor.target.Invoke(inst, args);
                }

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

        public bool IsSubclassOf(HybType other)
        {
            if (other.isCompiledType)
            {
                if (isCompiledType)
                {
                    return compiledType
                        .IsSubclassOf(other.compiledType);
                }
                return false;
            }
            return false;
        }
        public bool IsAssignableFrom(HybType other)
        {
            if (other.isCompiledType)
            {
                if (isCompiledType)
                {
                    return compiledType
                        .IsAssignableFrom(other.compiledType);
                }
                return false;
            }
            return false;
        }

        public override string ToString()
        {
            if (isCompiledType)
                return compiledType.Name;
            return interpretKlass.id;
        }
    }
}
