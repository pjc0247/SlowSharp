using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class VarFrame
    {
        public VarFrame parent { get; }

        private Dictionary<string, object> values = new Dictionary<string, object>();

        public VarFrame(VarFrame parent)
        {
            this.parent = parent;
        }

        public object GetValue(string key)
        {
            object v = null;
            if (TryGetValue(key, out v))
                return v;
            throw new ArgumentException(key);
        }
        public bool TryGetValue(string key, out object value)
        {
            if (values.ContainsKey(key))
            {
                value = values[key];
                return true;
            }

            if (parent == null)
            {
                value = null;
                return false;
            }

            return parent.TryGetValue(key, out value);
        }

        public void SetValue(string key, object value)
        {
            Console.WriteLine($"{key} = {value}");

            if (SetValueUpwards(key, value) == false)
                values[key] = value;
        }
        private bool SetValueUpwards(string key, object value)
        {
            if (values.ContainsKey(key))
            {
                values[key] = value;
                return true;
            }

            if (parent == null) return false;
            return parent.SetValueUpwards(key, value);
        }
    }
}
