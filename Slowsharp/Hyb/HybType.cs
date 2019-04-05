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

        public string id { get; }
        public bool isCompiledType => compiledType != null;
        public bool isInterface { get; }
        public bool isSealed { get; }

        public Type compiledType { get; }
        internal Class interpretKlass { get; }

        public bool isArray { get; }
        public int arrayRank { get; }
        public HybType elementType { get; }
        public HybType parent
        {
            get
            {
                if (_parent == null)
                {
                    if (isCompiledType)
                    {
                        _parent = compiledType.BaseType != null ?
                            new HybType(compiledType.BaseType) :
                            null;
                    }
                    else
                        _parent = interpretKlass.parent;
                }
                return _parent;
            }
        }
        private HybType _parent;

        internal HybType(Type type)
        {
            this.id = type.Name;
            this.isSealed = type.IsSealed;
            this.isInterface = type.IsInterface;
            this.compiledType = type;
        }
        internal HybType(Class klass)
        {
            this.id = klass.id;
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

        public HybInstance Override(Runner runner, HybInstance[] args, object parentObject)
        {
            return CreateInstanceInterpretType(runner, args, parentObject);
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
                return CreateInstanceInterpretType(runner, args);
        }
        private HybInstance CreateInstanceInterpretType(Runner runner, HybInstance[] args,
             object parentObject = null)
        {
            var inst = new HybInstance(runner, this, interpretKlass, parentObject);
            var ctors = inst.GetMethods("$_ctor");

            if (ctors.Length > 0)
            {
                var ctor = OverloadingResolver
                    .FindMethodWithArguments(runner.resolver, ctors, ref args);
                ctor.target.Invoke(inst, args);
            }

            return inst;
        }

        public bool SetStaticPropertyOrField(string id, HybInstance value)
        {
            if (isCompiledType)
            {
                var property = compiledType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (property != null)
                {
                    property.SetValue(null, value.Unwrap());
                    return true;
                }

                var field = compiledType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (field != null)
                {
                    field.SetValue(null, value.Unwrap());
                    return true;
                }
            }
            else
            {
                if (interpretKlass.HasStaticProperty(id))
                {
                    value = interpretKlass
                        .GetProperty(id)
                        .setMethod
                        .Invoke(null, new HybInstance[] { value });
                    return true;
                }
                if (interpretKlass.HasStaticField(id))
                {
                    interpretKlass.runner.globals.SetStaticField(
                        interpretKlass, id, value);
                    return true;
                }
            }

            value = null;
            return false;
        }
        public bool GetStaticPropertyOrField(string id, out HybInstance value)
        {
            if (isCompiledType)
            {
                var property = compiledType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (property != null)
                {
                    value = property.GetValue(null).Wrap();
                    return true;
                }

                var field = compiledType.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (field != null)
                {
                    value = field.GetValue(null).Wrap();
                    return true;
                }
            }
            else
            {
                if (interpretKlass.HasStaticProperty(id))
                {
                    value = interpretKlass
                        .GetProperty(id)
                        .getMethod
                        .Invoke(null, new HybInstance[] { });
                    return true;
                }
                if (interpretKlass.HasStaticField(id))
                {
                    value = interpretKlass.runner.globals.GetStaticField(
                        interpretKlass, id);
                    return true;
                }
            }

            value = null;
            return false;
        }
        public SSMethodInfo[] GetStaticMethods()
        {
            if (isCompiledType)
            {
                return compiledType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                   .Select(x => new SSMethodInfo(x)
                   {
                       id = x.Name,
                       isStatic = x.IsStatic,
                       accessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                if (parent == null)
                    return interpretKlass.GetMethods();
                return interpretKlass.GetMethods()
                    .Concat(parent.GetStaticMethods())
                    .ToArray();
            }
        }
        public SSMethodInfo[] GetStaticMethods(string id)
        {
            return GetStaticMethods()
                .Where(x => x.id == id)
                .ToArray();
        }
        public SSMethodInfo[] GetMethods()
        {
            if (isCompiledType)
            {
                return compiledType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                   .Where(x => x.IsPrivate == false)
                   .Select(x => new SSMethodInfo(x)
                   {
                       id = x.Name,
                       isStatic = x.IsStatic,
                       accessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                if (parent == null)
                    return interpretKlass.GetMethods();

                return parent.GetMethods()
                    .Concat(interpretKlass.GetMethods())
                    .ToArray();
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
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is HybType type)
            {
                if (isCompiledType)
                {
                    if (type.isCompiledType == false)
                        return false;
                    return compiledType == type.compiledType;
                }
                else
                {
                    return interpretKlass == type.interpretKlass;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            if (isCompiledType) return compiledType.GetHashCode();
            return interpretKlass.GetHashCode();
        }

        public static bool operator ==(HybType obj1, HybType obj2)
        {
            if (null == (object)obj1) return null == (object)obj2;
            return obj1.Equals(obj2);
        }
        public static bool operator !=(HybType obj1, HybType obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
