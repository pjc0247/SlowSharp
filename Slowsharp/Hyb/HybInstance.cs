using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public sealed class HybInstance
    {
        public string Id { get; private set; }

        public bool IsCompiledType => Type.IsCompiledType;
        public bool IsVirtualDerived
        {
            get
            {
                if (IsCompiledType == false &&
                    Parent?.IsCompiledType != null)
                    return true;
                return false;
            }
        }

        public object InnerObject
        {
            get
            {
                if (IsCompiledType == false)
                    throw new InvalidOperationException("Object is not a compiled type");
                return Obj;
            }
        }
        public HybInstance Parent { get; private set; }

        //private Dictionary<string, HybInstance> fields = new Dictionary<string, HybInstance>();
        internal PointableDictionary<string, HybInstance> Fields = new PointableDictionary<string, HybInstance>();

        private HybType Type;
        private object Obj;
        private Class Klass;
        private Runner Runner;

        public static HybInstance Null()
            => new HybInstance(HybType.Object, null);
        public static HybInstance Object(object o)
        {
            if (o == null)
                return Null();
            if (o is HybInstance hyb)
                return hyb;
            return new HybInstance(HybTypeCache.GetHybType(o.GetType()), o);
        }
        public static HybInstance ObjectArray(object[] o)
            => new HybInstance(new HybType(typeof(object[])), o);
        public static HybInstance ObjectArray(HybInstance[] o)
            => new HybInstance(new HybType(typeof(object[])), o.Unwrap());
        public static HybInstance Char(char c)
            => new HybInstance(HybType.Char, c);
        public static HybInstance String(string str)
            => new HybInstance(HybType.String, str);
        public static HybInstance Bool(bool b)
            => new HybInstance(HybType.Bool, b);
        public static HybInstance Byte(int n)
            => new HybInstance(HybType.Byte, n);
        public static HybInstance Short(int n)
            => new HybInstance(HybType.Short, n);
        public static HybInstance Int(int n)
            => new HybInstance(HybType.Int32, n);
        public static HybInstance Int64(Int64 n)
            => new HybInstance(HybType.Int64, n);
        public static HybInstance UInt(uint n)
            => new HybInstance(HybType.Uint32, n);
        public static HybInstance UInt64(UInt64 n)
            => new HybInstance(HybType.Uint64, n);
        public static HybInstance Float(float f)
            => new HybInstance(HybType.Float, f);
        public static HybInstance Double(double f)
            => new HybInstance(HybType.Double, f);
        public static HybInstance Decimal(decimal d)
            => new HybInstance(HybType.Decimal, d);
        public static HybInstance FromType(Type type)
            => new HybInstance(HybType.Type, type);

        internal HybInstance(HybType type, object obj)
        {
            this.Type = type;
            this.Obj = obj;
        }
        internal HybInstance(Runner runner, HybType type, Class klass, object parentObject = null)
        {
            if (klass == null)
                throw new ArgumentNullException(nameof(klass));

            this.Runner = runner;
            this.Type = type;
            this.Klass = klass;

            if (klass.Parent != null)
            {
                if (parentObject != null)
                    Parent = HybInstance.Object(parentObject);
                else
                    InstantiateParent();
            }

            InitializeFields();
            InitializeProperties();
        }
        private void InstantiateParent()
        {
            if (Klass.Parent.IsCompiledType)
                Parent = Klass.Parent.CreateInstance(Runner, new HybInstance[] { });
        }
        private void InitializeProperties()
        {
            foreach (var property in Klass.GetProperties()
                .Where(x => x.IsStatic == false)
                .OfType<SSInterpretPropertyInfo>())
            {
                if (property.Initializer != null)
                {
                    Runner.BindThis(this);
                    property.BackingField.SetValue(this,
                        Runner.RunExpression(property.Initializer.Value));
                }
            }
        }
        private void InitializeFields()
        {
            foreach (var field in Klass.GetFields()
                .Where(x => x.IsStatic == false)
                .OfType<SSInterpretFieldInfo>())
            {
                if (field.declartor == null || field.declartor.Initializer == null)
                {
                    Fields.Add(field.Id, 
                        HybInstance.Object(field.fieldType.GetDefault()));
                }
                else
                {
                    Fields.Add(field.Id,
                        Runner.RunExpression(field.declartor.Initializer.Value));
                }
            }
        }

        internal void SetIdentifier(string id) => this.Id = id;

        public HybType GetHybType() => Type;

        public HybInstance Cast<T>()
        {
            if (IsCompiledType)
                return HybInstance.Object((T)Convert.ChangeType(Obj, typeof(T)));
            if (Is<T>())
                return Parent.Cast<T>();
            throw new InvalidCastException($"Cannot be casted to {typeof(T)}");
        }
        public HybInstance Cast(HybType type)
        {
            if (IsCompiledType) {
                if (type.IsCompiledType)
                {
                    var casted = Convert.ChangeType(Obj, type.CompiledType);
                    return HybInstance.Object(casted);
                }
                throw new InvalidCastException(
                    $"{Obj} cannot be casted to {type.InterpretKlass.Id}");
            }

            throw new NotImplementedException();
        }

        public bool Is(HybType type)
        {
            if (type.IsCompiledType)
                return Is(type.CompiledType);
            return false;
        }
        public bool Is(Type type)
        {
            if (IsCompiledType)
            {
                var ct = GetHybType().CompiledType;
                if (ct == type || ct.IsSubclassOf(type))
                    return true;
                return false;
            }
            if (Klass.Parent == null)
                return false;

            // It's safe to access `compiledType` 
            //   without checking `isCompiledType`
            return 
                Klass.Parent.CompiledType == type ||
                Klass.Parent.IsSubclassOf(type);
        }
        public bool Is<T>() => Is(typeof(T));

        public object As(Type type)
        {
            if (IsCompiledType)
            {
                if (Obj.GetType() == type) return Obj;
                return Convert.ChangeType(Obj, type);
            }
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }
        public T As<T>()
        {
            if (IsCompiledType)
            {
                if (Obj is T tobj) return tobj;
                return (T)Convert.ChangeType(Obj, typeof(T));
            }
            return default(T);
        }

        public HybInstance Invoke(string name, params object[] args)
        {
            var wrappedArgs = args.Wrap();
            var methods = GetMethods(name);

            if (methods.Length == 0)
                throw new ArgumentException($"No such method: {name}");

            var method = OverloadingResolver.FindMethodWithArguments(
                Runner.Resolver, methods, new HybType[] { },ref wrappedArgs);

            if (method == null)
                throw new ArgumentException($"No matching override found: {name}");

            if (method.DeclaringType == Type)
                return method.Target.Invoke(this, wrappedArgs);
            return method.Target.Invoke(Parent, wrappedArgs);
        }

        public SSMethodInfo[] GetMethods(string id)
        {
            if (IsCompiledType)
            {
                return Obj.GetType().GetMethods()
                   .Where(x => x.Name == id)
                   .Select(x => new SSMethodInfo(x) {
                       Id = x.Name,
                       IsStatic = x.IsStatic,
                       AccessModifier = AccessModifierParser.Get(x)
                   })
                   .ToArray();
            }
            else
            {
                if (Parent == null)
                    return Klass.GetMethods(id);

                return Parent.GetMethods(id)
                    .Concat(Klass.GetMethods(id))
                    .ToArray();
            }
        }
        public SSMethodInfo GetSetIndexerMethod()
        {
            var idxer = GetIndexerProperty();
            if (idxer == null) return null;
            return new SSMethodInfo(idxer.GetSetMethod());
        }
        public SSMethodInfo GetGetIndexerMethod()
        {
            var idxer = GetIndexerProperty();
            if (idxer == null) return null;
            return new SSMethodInfo(idxer.GetGetMethod());
        }
        private PropertyInfo GetIndexerProperty()
        {
            return Obj.GetType()
               .GetProperties()
               .Where(x => x.GetIndexParameters().Length > 0)
               .FirstOrDefault();
        }

        public bool SetIndexer(object[] args, HybInstance value)
        {
            return SetIndexer(args.Wrap(), value);
        }
        public bool SetIndexer(HybInstance[] args, HybInstance value)
        {
            if (IsCompiledType)
            {
                if (Obj is Array ary)
                {
                    ary.SetValue(
                        value.Unwrap(),
                        args.Unwrap()
                            .Select(x => (int)x)
                            .ToArray());
                    return true;
                }

                var idxer = GetIndexerProperty();
                if (idxer == null)
                    return false;

                idxer.SetValue(Obj, value.As(idxer.PropertyType), args.Unwrap());
                return true;
            }
            else
            {
                var p = Type.GetProperty("[]");
                if (p != null)
                {
                    value = p.SetMethod.Invoke(this, args.Concat(new HybInstance[] { value }).ToArray());
                    return true;
                }

                if (Parent != null)
                    return Parent.SetIndexer(args, value);
            }

            return false;
        }
        public bool GetIndexer(object[] args, out HybInstance value)
        {
            return GetIndexer(args.Wrap(), out value);
        }
        public bool GetIndexer(HybInstance[] args, out HybInstance value)
        {
            value = null;

            if (IsCompiledType)
            {
                if (Obj is Array ary)
                {
                    value = HybInstance.Object(
                        ary.GetValue(args
                            .Unwrap()
                            .Select(x => (int)x)
                            .ToArray()));
                    return true;
                }

                var idxer = GetIndexerProperty();
                if (idxer == null)
                    return false;

                value = HybInstance.Object(
                    idxer.GetValue(Obj, args.Unwrap()));
                return true;
            }
            else
            {
                var p = Type.GetProperty("[]");
                if (p != null)
                {
                    value = p.GetMethod.Invoke(this, args);
                    return true;
                }

                if (Parent != null)
                    return Parent.GetIndexer(args, out value);
            }

            return false;
        }

        public bool SetPropertyOrField(string id, HybInstance value)
        {
            return SetPropertyOrField(id, value, AccessLevel.Outside);
        }
        internal bool SetPropertyOrField(string id, HybInstance value, AccessLevel level)
        {
            if (IsCompiledType)
            {
                var p = Obj.GetType()
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.SetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    p.SetValue(Obj, value.Unwrap());
                    return true;
                }

                var f = Obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (f != null)
                {
                    var mod = AccessModifierParser.Get(f);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    f.SetValue(Obj, value.Unwrap());
                    return true;
                }

                return false;
            }
            else
            {
                if (Klass.HasProperty(id, MemberFlag.Member))
                {
                    var p = Klass.GetProperty(id);
                    if (p.AccessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    Runner.BindThis(this);
                    value = p.SetMethod.Invoke(this, new HybInstance[] { value });
                    return true;
                }

                if (Klass.HasField(id, MemberFlag.Member))
                {
                    var f = Klass.GetField(id);
                    if (f.AccessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    Fields.Add(id, value);
                    return true;
                }

                if (Parent != null)
                    return Parent.SetPropertyOrField(id, value, level);

                return false;
            }
        }

        public bool GetPropertyOrField(string id, out HybInstance value)
        {
            return GetPropertyOrField(id, out value, AccessLevel.Outside);
        }
        internal bool GetPropertyOrField(string id, out HybInstance value, AccessLevel level)
        {
            if (IsCompiledType)
            {
                var p = Obj.GetType()
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.GetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(p.GetValue(Obj));
                    return true;
                }

                var f = Obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (f != null)
                {
                    var mod = AccessModifierParser.Get(f);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(f.GetValue(Obj));
                    return true;
                }

                value = null;
                return false;
            }
            else
            {
                if (Klass.HasProperty(id, MemberFlag.Member))
                {
                    var p = Klass.GetProperty(id);
                    if (p.AccessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    Runner.BindThis(this);
                    value = p.GetMethod.Invoke(this, new HybInstance[] { });
                    return true;
                }

                if (Klass.HasField(id, MemberFlag.Member))
                {
                    var f = Klass.GetField(id);
                    if (f.AccessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    value = Fields.Get(id);
                    return true;
                }

                if (Parent != null)
                    return Parent.GetPropertyOrField(id, out value, level);

                value = null;
                return false;
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (IsCompiledType)
            {
                if (Obj is IEnumerable e)
                    return e.GetEnumerator();
                if (Obj is IEnumerator et)
                    return et;
                throw new InvalidOperationException("Object is not an enumerable.");
            }

            throw new InvalidOperationException("Object is not an enumerable.");
        }

        public bool IsNull()
        {
            if (IsCompiledType) return Obj == null;
            return false;
        }
        public override string ToString()
        {
            if (IsCompiledType)
            {
                if (Obj == null)
                    return "null";
                return Obj.ToString();
            }
            return Type.ToString();
        }
        public DynamicHybInstance AsDynamic()
            => new DynamicHybInstance(this);

        internal HybInstance Clone()
        {
            if (IsCompiledType)
                return new HybInstance(Type, Obj);
            return new HybInstance(Runner, Type, Klass);
        }
    }
}
