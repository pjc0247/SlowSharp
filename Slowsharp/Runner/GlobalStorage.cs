using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class GlobalStorage
    {
        public Dictionary<string, HybInstance> storage = new Dictionary<string, HybInstance>();

        public void SetStaticField(Class klass, string id, HybInstance value)
        {
            storage[GetFieldSignature(klass, id)] = value;
        }
        public HybInstance GetStaticField(Class klass, string id)
        {
            return storage[GetFieldSignature(klass, id)];
        }
        private string GetFieldSignature(Class klass, string id)
        {
            return $"{klass.id}_f_{id}";
        }
    }
}
