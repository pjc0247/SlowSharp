using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class VarFrame
    {
        public VarFrame parent { get; }

        private Dictionary<string, HybInstance> values = new Dictionary<string, HybInstance>();

        public VarFrame(VarFrame parent)
        {
            this.parent = parent;
        }

        public HybInstance GetValue(string key)
        {
            HybInstance v = null;
            if (TryGetValue(key, out v))
                return v;
            throw new ArgumentException(key);
        }
        public bool TryGetValue(string key, out HybInstance value)
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

        public void SetValue(string key, HybInstance value)
        {
            Console.WriteLine($"{key} = {value}");

            if (SetValueUpwards(key, value) == false)
                values[key] = value;
        }
        private bool SetValueUpwards(string key, HybInstance value)
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
