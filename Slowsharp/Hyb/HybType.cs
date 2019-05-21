using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public sealed class HybType
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
        public static HybType Type => HybTypeCache.Type;

        public string Id { get; }
        public string FullName { get; }
        public bool IsCompiledType => CompiledType != null;
        public bool IsInterface { get; }
        public bool IsSealed { get; }
        public bool IsValueType { get; }
        public bool IsPrimitive { get; }

        public Type CompiledType { get; }
        internal Class InterpretKlass { get; }

        public bool IsArray { get; }
        public int ArrayRank { get; }
        public HybType ElementType { get; }
        public HybType[] Interfaces
        {
            get
            {
                if (_Interfaces == null)
                {
                    if (IsCompiledType)
                    {
                        _Interfaces = CompiledType.GetInterfaces()
                            .Select(x => HybTypeCache.GetHybType(x))
                            .ToArray();
                    }
                    else
                        _Interfaces = InterpretKlass.Interfaces;
                }
                return _Interfaces;
            }
        }
        private HybType[] _Interfaces;
        public HybType Parent
        {
            get
            {
                if (_Parent == null)
                {
                    if (IsCompiledType)
                    {
                        _Parent = CompiledType.BaseType != null ?
                            HybTypeCache.GetHybType(CompiledType.BaseType) :
                            null;
                    }
                    else
                        _Parent = InterpretKlass.Parent;
                }
                return _Parent;
            }
        }
        private HybType _Parent;

        internal HybType(Type type)
        {
            this.Id = type.Name;
            this.FullName = type.FullName;
            this.IsSealed = type.IsSealed;
            this.IsInterface = type.IsInterface;
            this.IsValueType = type.IsValueType;
            this.IsPrimitive = type.IsPrimitive;
            this.CompiledType = type;
        }
        internal HybType(Class klass)
        {
            this.Id = klass.Id;
            this.FullName = klass.Id;
            this.IsSealed = false;
            this.IsInterface = false;
            this.IsValueType = false;
            this.IsPrimitive = false;
            this.InterpretKlass = klass;
        }
        internal HybType(Class klass, HybType elementType, int arrayRank) :
            this(klass)
        {
            this.IsArray = true;
            this.ElementType = elementType;
            this.ArrayRank = arrayRank;
        }

        public HybType MakeArrayType(int rank)
        {
            if (rank < 1)
                throw new ArgumentException(nameof(rank));

            if (IsCompiledType)
            {
                if (rank == 1)
                    return HybTypeCache.GetHybType(CompiledType.MakeArrayType());
                return HybTypeCache.GetHybType(CompiledType.MakeArrayType(rank));
            }
            return new HybType(InterpretKlass, this, ArrayRank);
        }
        public HybType MakeGenericType(HybType[] genericArgs)
        {
            if (genericArgs == null)
                throw new ArgumentNullException(nameof(genericArgs));

            if (IsCompiledType)
            {
                return HybTypeCache.GetHybType(CompiledType.MakeGenericType(genericArgs.Unwrap()));
            }
            return null;
        }

        public HybInstance Override(Runner runner, HybInstance[] args, object parentObject)
        {
            return CreateInstanceInterpretType(runner, args, parentObject);
        }
        public HybInstance CreateInstance(Runner runner, HybInstance[] args)
        {
            if (IsCompiledType)
            {
                return new HybInstance(
                    this,
                    Activator.CreateInstance(CompiledType, args.Unwrap()));
            }
            // Array with interpret type
            else if (IsArray)
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
            var inst = new HybInstance(runner, this, InterpretKlass, parentObject);
            var ctors = GetMethods("$_ctor");

            if (ctors.Length > 0)
            {
                var ctor = OverloadingResolver
                    .FindMethodWithArguments(runner.Resolver, ctors, new HybType[] { }, ref args);
                ctor.Target.Invoke(inst, args);
            }

            return inst;
        }

        /// <summary>
        /// Returns whether type implements the given interface or not.
        /// </summary>
        public bool HasInterface(HybType type)
            => Interfaces.Any(x => x == type);
        /// <summary>
        /// Returns whether type implements the given interface or not.
        /// </summary>
        public bool HasInterface(Type type)
            => Interfaces.Any(x => x.IsCompiledType && x.CompiledType == type);

        public SSPropertyInfo GetProperty(string id)
        {
            SSPropertyInfo property = null;

            if (_Properties == null) _Properties = new Dictionary<string, SSPropertyInfo>();
            if (_Properties.ContainsKey(id))
                property = _Properties[id];
            else
            {
                property = _GetProperty(id);
                if (property != null)
                    _Properties[id] = property;
            }

            return property;
        }
        private Dictionary<string, SSPropertyInfo> _Properties = null;
        private SSPropertyInfo _GetProperty(string id)
        {
            if (IsCompiledType)
            {
                var property = CompiledType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (property != null)
                    return new SSCompiledPropertyInfo(property);
            }
            else
            {
                if (InterpretKlass.HasProperty(id))
                    return InterpretKlass.GetProperty(id);
                if (Parent != null)
                    return Parent.GetProperty(id);
            }

            return null;
        }

        public SSFieldInfo GetField(string id)
        {
            SSFieldInfo field = null;

            if (_Fields == null) _Fields = new Dictionary<string, SSFieldInfo>();
            if (_Fields.ContainsKey(id))
                field = _Fields[id];
            else
            {
                field = _GetField(id);
                if (field != null)
                    _Fields[id] = field;
            }

            return field;
        }
        private Dictionary<string, SSFieldInfo> _Fields = null;
        private SSFieldInfo _GetField(string id)
        {
            if (IsCompiledType)
            {
                var field = CompiledType.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (field != null)
                    return new SSCompiledFieldInfo(field);
            }
            else
            {
                if (InterpretKlass.HasField(id))
                    return InterpretKlass.GetField(id);
                if (Parent != null)
                    return Parent.GetField(id);
            }

            return null;
        }

        public bool SetStaticPropertyOrField(string id, HybInstance value)
        {
            return SetStaticPropertyOrField(id, value, AccessLevel.Outside);
        }
        internal bool SetStaticPropertyOrField(string id, HybInstance value, AccessLevel accessLevel)
        {
            SSPropertyInfo property = GetProperty(id);
            if (property != null)
            {
                if (property.AccessModifier.IsAcceesible(accessLevel) == false)
                    throw new SemanticViolationException($"Invalid access: {id}");
                property.SetMethod.Invoke(null, new HybInstance[] { value });
                return true;
            }

            SSFieldInfo field = GetField(id);
            if (field != null)
            {
                if (field.AccessModifier.IsAcceesible(accessLevel) == false)
                    throw new SemanticViolationException($"Invalid access: {id}");
                field.SetValue(null, value);
                return true;
            }

            return false;
        }

        public bool GetStaticPropertyOrField(string id, out HybInstance value)
        {
            return GetStaticPropertyOrField(id, out value, AccessLevel.Outside);
        }
        internal bool GetStaticPropertyOrField(string id, out HybInstance value, AccessLevel accessLevel)
        {
            SSPropertyInfo property = GetProperty(id);
            if (property != null)
            {
                if (property.AccessModifier.IsAcceesible(accessLevel) == false)
                    throw new SemanticViolationException($"Invalid access: {id}");
                value = property.GetMethod.Invoke(null, new HybInstance[] { });
                return true;
            }

            SSFieldInfo field = GetField(id);
            if (field != null)
            {
                if (field.AccessModifier.IsAcceesible(accessLevel) == false)
                    throw new SemanticViolationException($"Invalid access: {id}");
                value = field.GetValue(null);
                return true;
            }

            value = null;
            return false;
        }

        public SSMethodInfo[] GetStaticMethods(string id)
        {
            return GetStaticMethods()
                .Where(x => x.Id == id)
                .ToArray();
        }
        internal SSMethodInfo GetStaticMethodFirst(string id)
        {
            return GetStaticMethods().Where(x => x.Id == id).FirstOrDefault();
        }
        public SSMethodInfo[] GetStaticMethods()
        {
            if (_StaticMethods == null)
                _StaticMethods = _GetStaticMethods();
            return _StaticMethods;
        }
        private SSMethodInfo[] _StaticMethods;
        private SSMethodInfo[] _GetStaticMethods()
        {
            if (IsCompiledType)
            {
                return CompiledType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                   .Select(x => new SSMethodInfo(x)
                   {
                       Id = x.Name,
                       IsStatic = x.IsStatic,
                       AccessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                if (Parent == null)
                    return InterpretKlass.GetMethods();
                return InterpretKlass.GetMethods()
                    .Concat(Parent.GetStaticMethods())
                    .ToArray();
            }
        }

        public SSMethodInfo[] GetMethods(string id)
        {
            return GetMethods()
                .Where(x => x.Id == id)
                .ToArray();
        }
        internal SSMethodInfo GetMethodFirst(string id)
        {
            return GetMethods().Where(x => x.Id == id).FirstOrDefault();
        }
        public SSMethodInfo[] GetMethods()
        {
            if (_Methods == null)
                _Methods = _GetMethods();
            return _Methods;
        }
        private SSMethodInfo[] _Methods;
        private SSMethodInfo[] _GetMethods()
        {
            if (IsCompiledType)
            {
                return CompiledType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                   .Where(x => x.IsPrivate == false)
                   .Select(x => new SSMethodInfo(x)
                   {
                       Id = x.Name,
                       IsStatic = x.IsStatic,
                       AccessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                if (Parent == null)
                    return InterpretKlass.GetMethods();

                return Parent.GetMethods()
                    .Concat(InterpretKlass.GetMethods())
                    .GroupBy(x => x.Signature)
                    .Select(x => x.Last())
                    .ToArray();
            }
        }

        public HybInstance GetDefault()
        {
            if (IsCompiledType)
            {
                if (CompiledType.IsValueType)
                    return HybInstance.Object(Activator.CreateInstance(CompiledType));
                return HybInstance.Null();
            }
            return HybInstance.Null();
        }

        public bool IsSubclassOf(HybType other)
        {
            if (other.IsCompiledType)
            {
                if (IsCompiledType)
                {
                    return CompiledType
                        .IsSubclassOf(other.CompiledType);
                }
                return false;
            }
            return false;
        }
        public bool IsSubclassOf(Type other)
        {
            if (IsCompiledType)
                return CompiledType.IsSubclassOf(other);

            var type = Parent;
            while (type != null)
            {
                if (type.IsCompiledType &&
                    type.CompiledType == other) return true;
                type = type.Parent;
            }
            return false;
        }
        public bool IsAssignableFrom(HybType other)
        {
            if (other.IsCompiledType)
            {
                if (IsCompiledType)
                {
                    if (CompiledType.IsPrimitive && other.CompiledType.IsPrimitive)
                        return TypeDescriptor.GetConverter(other.CompiledType)
                            .CanConvertTo(CompiledType);

                    return CompiledType
                        .IsAssignableFrom(other.CompiledType);
                }
                return false;
            }
            return false;
        }

        public override string ToString()
        {
            if (IsCompiledType)
                return CompiledType.Name;
            return InterpretKlass.Id;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is HybType type)
            {
                if (IsCompiledType)
                {
                    if (type.IsCompiledType == false)
                        return false;
                    return CompiledType == type.CompiledType;
                }
                else
                {
                    return InterpretKlass == type.InterpretKlass;
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            if (IsCompiledType) return CompiledType.GetHashCode();
            return InterpretKlass.GetHashCode();
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
