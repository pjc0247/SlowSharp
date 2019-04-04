using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class HybInstance
    {
        public bool isCompiledType => type.isCompiledType;

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

        private Dictionary<string, HybInstance> fields = new Dictionary<string, HybInstance>();

        private HybType type;
        private object obj;
        private Class klass;
        private Runner runner;

        public static HybInstance Null()
        {
            return new HybInstance(HybType.Object, null);
        }
        public static HybInstance Object(object o)
        {
            if (o == null)
                return Null();
            if (o is HybInstance hyb)
                return hyb;
            return new HybInstance(new HybType(o.GetType()), o);
        }
        public static HybInstance ObjectArray(object[] o)
        {
            return new HybInstance(new HybType(typeof(object[])), o);
        }
        public static HybInstance ObjectArray(HybInstance[] o)
        {
            return new HybInstance(new HybType(typeof(object[])), o.Unwrap());
        }
        public static HybInstance Char(char c)
        {
            return new HybInstance(HybType.Char, c);
        }
        public static HybInstance String(string str)
        {
            return new HybInstance(HybType.String, str);
        }
        public static HybInstance Bool(bool b)
        {
            return new HybInstance(HybType.Bool, b);
        }
        public static HybInstance Int(int n)
        {
            return new HybInstance(HybType.Int32, n);
        }
        public static HybInstance Int64(Int64 n)
        {
            return new HybInstance(HybType.Int64, n);
        }
        public static HybInstance Float(float f)
        {
            return new HybInstance(HybType.Float, f);
        }
        public static HybInstance Double(double f)
        {
            return new HybInstance(HybType.Double, f);
        }

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
                .Where(x => x.isStatic == false))
            {
                if (property.property.Initializer != null)
                {
                    runner.BindThis(this);
                    property.setMethod
                        .Invoke(this, new HybInstance[] {
                            runner.RunExpression(property.property.Initializer.Value)
                        });
                }
            }
        }
        private void InitializeFields()
        {
            foreach (var field in klass.GetFields()
                .Where(x => x.isStatic == false))
            {
                if (field.declartor == null || field.declartor.Initializer == null)
                {
                    fields.Add(field.id, HybInstance.Object(field.fieldType.GetDefault()));
                }
                else
                {
                    fields.Add(field.id,
                        runner.RunExpression(field.declartor.Initializer.Value));
                }
            }
        }

        public HybType GetHybType() => type;

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
            }
            return false;
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
                runner.resolver, methods, wrappedArgs);

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
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.SetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    p.SetValue(obj, value.Unwrap());
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

                    fields[id] = value;
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
                   .GetProperty(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.GetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(p.GetValue(obj));
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
                    value = fields[id];
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
        public HybInstance Clone()
        {
            if (isCompiledType)
                return new HybInstance(type, obj);
            return new HybInstance(runner, type, klass);
        }

        public static HybInstance operator ++(HybInstance _this)
        {
            var c = _this.Clone();
            c += 1;
            return c;
        }
        public static HybInstance operator +(HybInstance _this, int n)
        {
            if (_this.isCompiledType)
            {
                if (_this.obj is int i) _this.obj = i + n;
                else if (_this.obj is uint ui) _this.obj = ui + n;
                else if (_this.obj is float f) _this.obj = f + n;
                else if (_this.obj is double d) _this.obj = d + n;
                else if (_this.obj is decimal dec) _this.obj = dec + n;
            }

            return _this;
        }
        public static HybInstance operator +(HybInstance _this, string n)
        {
            if (_this.isCompiledType)
            {
                if (_this.obj is string s) _this.obj = s + n;
            }

            return _this;
        }
    }
}
