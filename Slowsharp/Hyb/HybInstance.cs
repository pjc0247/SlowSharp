using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class HybInstance
    {
        public bool isCompiledType => obj != null;

        public object innerObject
        {
            get
            {
                if (isCompiledType == false)
                    throw new InvalidOperationException("Object is not a compiled type");
                return obj;
            }
        }

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
            return new HybInstance(new HybType(o.GetType()), o);
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
            return new HybInstance(HybType.Int, n);
        }
        public static HybInstance Float(float f)
        {
            return new HybInstance(HybType.Float, f);
        }

        public HybInstance(HybType type, object obj)
        {
            this.type = type;
            this.obj = obj;
        }
        public HybInstance(Runner runner, HybType type, Class klass)
        {
            this.runner = runner;
            this.type = type;
            this.klass = klass;

            foreach (var field in klass.GetFields())
            {
                if (field.declartor.Initializer == null)
                {
                    var hybType = runner.resolver.GetType($"{field.field.Declaration.Type}");
                    fields.Add(field.id, HybInstance.Object(hybType.GetDefault()));
                }
                else
                {
                    fields.Add(field.id,
                        runner.RunExpression(field.declartor.Initializer.Value));
                }
            }
        }

        public HybType GetHybType() => type;

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

        public Invokable[] GetMethods(string id)
        {
            if (isCompiledType)
            {
                return obj.GetType().GetMethods()
                   .Where(x => x.Name == id)
                   .Select(x => new Invokable(x))
                   .ToArray();
            }
            else
            {
                return klass.GetMethods(id);
            }
        }

        public bool SetIndexer(HybInstance[] args, HybInstance value)
        {
            if (isCompiledType)
            {
                if (obj is Array ary)
                {
                    ary.SetValue(
                        value,
                        args.Unwrap()
                            .Select(x => (int)x)
                            .ToArray());
                    return true;
                }

                var idxer = obj.GetType()
                   .GetProperties()
                   .Where(x => x.GetIndexParameters().Length > 0)
                   .FirstOrDefault();

                if (idxer == null)
                    return false;

                idxer.SetValue(obj, value.As(idxer.PropertyType), args.Unwrap());
                return true;
            }

            return false;
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

                var idxer = obj.GetType()
                   .GetProperties()
                   .Where(x => x.GetIndexParameters().Length > 0)
                   .FirstOrDefault();

                if (idxer == null)
                    return false;

                value = HybInstance.Object(
                    idxer.GetValue(obj, args.Unwrap()));
                return true;
            }

            return false;
        }

        public bool SetPropertyOrField(string id, HybInstance value, AccessLevel level)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.SetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    p.SetValue(obj, value);
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id);
                if (f != null)
                {
                    var mod = AccessModifierParser.Get(f);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    f.SetValue(obj, value);
                    return true;
                }

                return false;
            }
            else
            {
                if (klass.HasField(id))
                {
                    var f = klass.GetField(id);
                    if (f.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    fields[id] = value;
                    return true;
                }

                return false;
            }
        }
        public bool GetPropertyOrField(string id, out HybInstance value, AccessLevel level)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id);
                if (p != null)
                {
                    var mod = AccessModifierParser.Get(p.GetMethod);
                    if (mod.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");

                    value = HybInstance.Object(p.GetValue(obj));
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id);
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
                if (klass.HasField(id))
                {
                    var f = klass.GetField(id);
                    if (f.accessModifier.IsAcceesible(level) == false)
                        throw new SemanticViolationException($"Invalid access: {id}");
                    value = fields[id];
                    return true;
                }

                value = null;
                return false;
            }
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
