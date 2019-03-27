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

        private Dictionary<string, object> fields = new Dictionary<string, object>();

        private HybType type;
        private object obj;
        private Class klass;

        public HybInstance(HybType type, object obj)
        {
            this.type = type;
            this.obj = obj;
        }
        public HybInstance(HybType type, Class klass)
        {
            this.type = type;
            this.klass = klass;
        }

        public HybType GetHybType() => type;

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

        public bool SetPropertyOrField(string id, object value)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id);
                if (p != null)
                {
                    p.SetValue(obj, value);
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id);
                if (f != null)
                {
                    f.SetValue(obj, value);
                    return true;
                }

                return false;
            }
            else
            {
                if (klass.HasField(id))
                {
                    fields[id] = value;
                    return true;
                }

                return false;
            }
        }

        public bool GetPropertyOrField(string id, out object value)
        {
            if (isCompiledType)
            {
                var p = obj.GetType()
                   .GetProperty(id);
                if (p != null)
                {
                    value = p.GetValue(obj);
                    return true;
                }

                var f = obj.GetType()
                    .GetField(id);
                if (f != null)
                {
                    value = f.GetValue(obj);
                    return true;
                }

                value = null;
                return false;
            }
            else
            {
                if (klass.HasField(id))
                {
                    value = fields[id];
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}
