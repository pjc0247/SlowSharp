using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class VarFrame
    {
        public VarFrame Parent { get; }

        private Dictionary<string, HybInstance> Values = new Dictionary<string, HybInstance>();

        public VarFrame(VarFrame parent)
        {
            this.Parent = parent;
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
            if (Values.ContainsKey(key))
            {
                value = Values[key];
                return true;
            }

            if (Parent == null)
            {
                value = null;
                return false;
            }

            return Parent.TryGetValue(key, out value);
        }

        public bool UpdateValue(string key, HybInstance value)
        {
            Console.WriteLine($"{key} = {value}");

            return SetValueUpwards(key, value);
        }
        public void SetValue(string key, HybInstance value)
        {
            Console.WriteLine($"{key} = {value}");

            if (SetValueUpwards(key, value) == false)
                Values[key] = value;
        }
        private bool SetValueUpwards(string key, HybInstance value)
        {
            if (Values.ContainsKey(key))
            {
                Values[key] = value;
                return true;
            }

            if (Parent == null) return false;
            return Parent.SetValueUpwards(key, value);
        }

        public Dictionary<string, HybInstance> Flatten()
        {
            var dict = new Dictionary<string, HybInstance>();
            FlattenUpwards(dict);
            return dict;
        }
        private void FlattenUpwards(Dictionary<string, HybInstance> dict)
        {
            foreach (var pair in Values)
                dict[pair.Key] = pair.Value;

            if (Parent != null)
                Parent.FlattenUpwards(dict);
        }
    }
}
