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

        public string id { get; }
        public string fullName { get; }
        public bool isCompiledType => compiledType != null;
        public bool isInterface { get; }
        public bool isSealed { get; }
        public bool isValueType { get; }
        public bool isPrimitive { get; }

        public Type compiledType { get; }
        internal Class interpretKlass { get; }

        public bool isArray { get; }
        public int arrayRank { get; }
        public HybType elementType { get; }
        public HybType[] interfaces
        {
            get
            {
                if (_interfaces == null)
                {
                    if (isCompiledType)
                    {
                        _interfaces = compiledType.GetInterfaces()
                            .Select(x => HybTypeCache.GetHybType(x))
                            .ToArray();
                    }
                    else
                        _interfaces = interpretKlass.interfaces;
                }
                return _interfaces;
            }
        }
        private HybType[] _interfaces;
        public HybType parent
        {
            get
            {
                if (_parent == null)
                {
                    if (isCompiledType)
                    {
                        _parent = compiledType.BaseType != null ?
                            HybTypeCache.GetHybType(compiledType.BaseType) :
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
            this.fullName = type.FullName;
            this.isSealed = type.IsSealed;
            this.isInterface = type.IsInterface;
            this.isValueType = type.IsValueType;
            this.isPrimitive = type.IsPrimitive;
            this.compiledType = type;
        }
        internal HybType(Class klass)
        {
            this.id = klass.id;
            this.fullName = klass.id;
            this.isSealed = false;
            this.isInterface = false;
            this.isValueType = false;
            this.isPrimitive = false;
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
            if (rank < 1)
                throw new ArgumentException(nameof(rank));

            if (isCompiledType)
            {
                if (rank == 1)
                    return HybTypeCache.GetHybType(compiledType.MakeArrayType());
                return HybTypeCache.GetHybType(compiledType.MakeArrayType(rank));
            }
            return new HybType(interpretKlass, this, arrayRank);
        }
        public HybType MakeGenericType(HybType[] genericArgs)
        {
            if (genericArgs == null)
                throw new ArgumentNullException(nameof(genericArgs));

            if (isCompiledType)
            {
                return HybTypeCache.GetHybType(compiledType.MakeGenericType(genericArgs.Unwrap()));
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
            var ctors = GetMethods("$_ctor");

            if (ctors.Length > 0)
            {
                var ctor = OverloadingResolver
                    .FindMethodWithArguments(runner.resolver, ctors, new HybType[] { }, ref args);
                ctor.target.Invoke(inst, args);
            }

            return inst;
        }

        /// <summary>
        /// Returns whether type implements the given interface or not.
        /// </summary>
        public bool HasInterface(HybType type)
            => interfaces.Any(x => x == type);
        /// <summary>
        /// Returns whether type implements the given interface or not.
        /// </summary>
        public bool HasInterface(Type type)
            => interfaces.Any(x => x.isCompiledType && x.compiledType == type);

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
            if (isCompiledType)
            {
                var property = compiledType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (property != null)
                    return new SSCompiledPropertyInfo(property);
            }
            else
            {
                if (interpretKlass.HasProperty(id))
                    return interpretKlass.GetProperty(id);
                if (parent != null)
                    return parent.GetProperty(id);
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
            if (isCompiledType)
            {
                var field = compiledType.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.Name == id)
                    .FirstOrDefault();
                if (field != null)
                    return new SSCompiledFieldInfo(field);
            }
            else
            {
                if (interpretKlass.HasField(id))
                    return interpretKlass.GetField(id);
                if (parent != null)
                    return parent.GetField(id);
            }

            return null;
        }

        public bool SetStaticPropertyOrField(string id, HybInstance value)
        {
            SSPropertyInfo property = GetProperty(id);
            if (property != null)
            {
                property.setMethod.Invoke(null, new HybInstance[] { value });
                return true;
            }

            SSFieldInfo field = GetField(id);
            if (field != null)
            {
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
                if (property.accessModifier.IsAcceesible(accessLevel) == false)
                    throw new SemanticViolationException($"Invalid access: {id}");
                value = property.getMethod.Invoke(null, new HybInstance[] { });
                return true;
            }

            SSFieldInfo field = GetField(id);
            if (field != null)
            {
                if (field.accessModifier.IsAcceesible(accessLevel) == false)
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
                .Where(x => x.id == id)
                .ToArray();
        }
        internal SSMethodInfo GetStaticMethodFirst(string id)
        {
            return GetStaticMethods().Where(x => x.id == id).FirstOrDefault();
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

        public SSMethodInfo[] GetMethods(string id)
        {
            return GetMethods()
                .Where(x => x.id == id)
                .ToArray();
        }
        internal SSMethodInfo GetMethodFirst(string id)
        {
            return GetMethods().Where(x => x.id == id).FirstOrDefault();
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
                    .GroupBy(x => x.signature)
                    .Select(x => x.Last())
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
        public bool IsSubclassOf(Type other)
        {
            if (isCompiledType)
                return compiledType.IsSubclassOf(other);

            var type = parent;
            while (type != null)
            {
                if (type.isCompiledType &&
                    type.compiledType == other) return true;
                type = type.parent;
            }
            return false;
        }
        public bool IsAssignableFrom(HybType other)
        {
            if (other.isCompiledType)
            {
                if (isCompiledType)
                {
                    if (compiledType.IsPrimitive && other.compiledType.IsPrimitive)
                        return TypeDescriptor.GetConverter(other.compiledType)
                            .CanConvertTo(compiledType);

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
