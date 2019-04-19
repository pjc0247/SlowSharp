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
        public string id { get; private set; }

        public bool isCompiledType => type.isCompiledType;
        public bool isVirtualDerived
        {
            get
            {
                if (isCompiledType == false &&
                    parent?.isCompiledType != null)
                    return true;
                return false;
            }
        }

        public object innerObject
        {
            get
            {
                if (isCompiledType == false)
                    throw new InvalidOperationException("Object is not a compiled type");
                return obj;
            }
        }
        public HybInstance parent { get; private set; }

        //private Dictionary<string, HybInstance> fields = new Dictionary<string, HybInstance>();
        internal PointableDictionary<string, HybInstance> fields = new PointableDictionary<string, HybInstance>();

        private HybType type;
        private object obj;
        private Class klass;
        private Runner runner;

        public static HybInstance Null()
            => new HybInstance(HybType.Object, null);
        public static HybInstance Object(object o)
        {
            if (o == null)
                return Null();
            if (o is HybInstance hyb)
                return hyb;
            return new HybInstance(new HybType(o.GetType()), o);
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
        public static HybInstance Float(float f)
            => new HybInstance(HybType.Float, f);
        public static HybInstance Double(double f)
            => new HybInstance(HybType.Double, f);
        public static HybInstance Decimal(decimal d)
            => new HybInstance(HybType.Decimal, d);
        public static HybInstance Type(Type type)
            => new HybInstance(HybType.Type, type);

        internal HybInstance(HybType type, object obj)
        {
            this.type = type;
            this.obj = obj;
        }
        internal HybInstance(Runner runner, HybType type, Class klass, object parentObject = null)
        {
            this.runner = runner;
            this.type = type;
            this.klass = klass;

            if (klass.parent != null)
            {
                if (parentObject != null)
                    parent = HybInstance.Object(parentObject);
                else
                    InstantiateParent();
            }

            InitializeFields();
            InitializeProperties();
        }
        private void InstantiateParent()
        {
            if (klass.parent.isCompiledType)
                parent = klass.parent.CreateInstance(runner, new HybInstance[] { });
        }
        private void InitializeProperties()
        {
            foreach (var property in klass.GetProperties()
                .Where(x => x.isStatic == false)
                .OfType<SSInterpretPropertyInfo>())
            {
                if (property.property.Initializer != null)
                {
                    runner.BindThis(this);
                    property.backingField.SetValue(this,
                        runner.RunExpression(property.property.Initializer.Value));
                }
            }
        }
        private void InitializeFields()
        {
            foreach (var field in klass.GetFields()
                .Where(x => x.isStatic == false)
                .OfType<SSInterpretFieldInfo>())
            {
                if (field.declartor == null || field.declartor.Initializer == null)
                {
                    fields.Add(field.id, 
                        HybInstance.Object(field.fieldType.GetDefault()));
                }
                else
                {
                    fields.Add(field.id,
                        runner.RunExpression(field.declartor.Initializer.Value));
                }
            }
        }

        internal void SetIdentifier(string id) => this.id = id;

        public HybType GetHybType() => type;

        public HybInstance Cast<T>()
        {
            if (isCompiledType)
                return HybInstance.Object((T)Convert.ChangeType(obj, typeof(T)));
            if (Is<T>())
                return parent.Cast<T>();
            throw new InvalidCastException($"Cannot be casted to {typeof(T)}");
        }
        public HybInstance Cast(HybType type)
        {
            if (isCompiledType) {
                if (type.isCompiledType)
                {
                    var casted = Convert.ChangeType(obj, type.compiledType);
                    return HybInstance.Object(casted);
                }
                throw new InvalidCastException(
                    $"{obj} cannot be casted to {type.interpretKlass.id}");
            }

            throw new NotImplementedException();
        }

        public bool Is(HybType type)
        {
            if (type.isCompiledType)
                return Is(type.compiledType);
            return false;
        }
        public bool Is(Type type)
        {
            if (isCompiledType)
            {
                var ct = GetHybType().compiledType;
                if (ct == type || ct.IsSubclassOf(type))
                    return true;
                return false;
            }
            if (klass.parent == null)
                return false;

            // It's safe to access `compiledType` 
            //   without checking `isCompiledType`
            return 
                klass.parent.compiledType == type ||
                klass.parent.IsSubclassOf(type);
        }
        public bool Is<T>() => Is(typeof(T));

        public object As(Type type)
        {
            if (isCompiledType)
            {
                if (obj.GetType() == type) return obj;
                return Convert.ChangeType(obj, type);
            }
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }
        public T As<T>()
        {
            if (isCompiledType)
            {
                if (obj is T tobj) return tobj;
                return (T)Convert.ChangeType(obj, typeof(T));
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
                runner.resolver, methods, new HybType[] { },ref wrappedArgs);

            if (method == null)
                throw new ArgumentException($"No matching override found: {name}");

            if (method.declaringType == type)
                return method.target.Invoke(this, wrappedArgs);
            return method.target.Invoke(parent, wrappedArgs);
        }

        public SSMethodInfo[] GetMethods(string id)
        {
            if (isCompiledType)
            {
                return obj.GetType().GetMethods()
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
                if (parent == null)
                    return klass.GetMethods(id);

                return parent.GetMethods(id)
                    .Concat(klass.GetMethods(id))
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
            return obj.GetType()
               .GetProperties()
               .Where(x => x.GetIndexParameters().Length > 0)
               .FirstOrDefault();
        }

        public bool SetIndexer(HybInstance[] args, HybInstance value)
        {
            if (isCompiledType)
            {
                if (obj is Array ary)
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

                idxer.SetValue(obj, value.As(idxer.PropertyType), args.Unwrap());
                return true;
            }
            else
            {
                if (parent != null)
                    return parent.SetIndexer(args, value);
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

            if (isCompiledType)
            {
                if (obj is Array ary)
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
                    idxer.GetValue(obj, args.Unwrap()));
                return true;
            }
            else
            {
                if (parent != null)
                    return parent.GetIndexer(args, out value);
            }

            return false;
        }

        public bool SetPropertyOrField(string id, HybInstance value)
        {
            return SetPropertyOrField(id, value, AccessLevel.Outside);
        }
        internal bool SetPropertyOrField(string id, HybInstance value, AccessLevel level)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.SetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    p.SetValue(obj, value.Unwrap());
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (f != null)
                {
                    var mod = AccessModifierParser.Get(f);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    f.SetValue(obj, value.Unwrap());
                    return true;
                }

                return false;
            }
            else
            {
                if (klass.HasProperty(id, MemberFlag.Member))
                {
                    var p = klass.GetProperty(id);
                    if (p.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    runner.BindThis(this);
                    value = p.setMethod.Invoke(this, new HybInstance[] { value });
                    return true;
                }

                if (klass.HasField(id, MemberFlag.Member))
                {
                    var f = klass.GetField(id);
                    if (f.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    fields.Add(id, value);
                    return true;
                }

                if (parent != null)
                    return parent.SetPropertyOrField(id, value, level);

                return false;
            }
        }

        public bool GetPropertyOrField(string id, out HybInstance value)
        {
            return GetPropertyOrField(id, out value, AccessLevel.Outside);
        }
        internal bool GetPropertyOrField(string id, out HybInstance value, AccessLevel level)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.GetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(p.GetValue(obj));
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (f != null)
                {
                    var mod = AccessModifierParser.Get(f);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(f.GetValue(obj));
                    return true;
                }

                value = null;
                return false;
            }
            else
            {
                if (klass.HasProperty(id, MemberFlag.Member))
                {
                    var p = klass.GetProperty(id);
                    if (p.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    runner.BindThis(this);
                    value = p.getMethod.Invoke(this, new HybInstance[] { });
                    return true;
                }

                if (klass.HasField(id, MemberFlag.Member))
                {
                    var f = klass.GetField(id);
                    if (f.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    value = fields.Get(id);
                    return true;
                }

                if (parent != null)
                    return parent.GetPropertyOrField(id, out value, level);

                value = null;
                return false;
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (isCompiledType)
            {
                if (obj is IEnumerable e)
                    return e.GetEnumerator();
                if (obj is IEnumerator et)
                    return et;
                throw new InvalidOperationException("Object is not an enumerable.");
            }

            throw new InvalidOperationException("Object is not an enumerable.");
        }

        public bool IsNull()
        {
            if (isCompiledType) return obj == null;
            return false;
        }
        public override string ToString()
        {
            if (isCompiledType)
            {
                if (obj == null)
                    return "null";
                return obj.ToString();
            }
            return type.ToString();
        }
        public DynamicHybInstance AsDynamic()
            => new DynamicHybInstance(this);

        internal HybInstance Clone()
        {
            if (isCompiledType)
                return new HybInstance(type, obj);
            return new HybInstance(runner, type, klass);
        }
    }
}
