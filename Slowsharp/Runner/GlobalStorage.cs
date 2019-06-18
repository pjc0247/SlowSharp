using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class GlobalStorage
    {
        public Dictionary<string, HybInstance> Storage = new Dictionary<string, HybInstance>();

        public void SetStaticField(Class klass, string id, HybInstance value)
        {
            C.WriteLine($"set_{GetFieldSignature(klass, id)} = {value}");

            Storage[GetFieldSignature(klass, id)] = value;
        }
        public HybInstance GetStaticField(Class klass, string id)
        {
            C.WriteLine($"get_{GetFieldSignature(klass, id)} => {Storage[GetFieldSignature(klass, id)]}");

            return Storage[GetFieldSignature(klass, id)];
        }
        private string GetFieldSignature(Class klass, string id)
        {
            return $"{klass.Id}_f_{id}";
        }
    }
}
