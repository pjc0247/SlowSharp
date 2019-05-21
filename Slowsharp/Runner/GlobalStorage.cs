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
#if SS_TRACE
            Console.WriteLine($"set_{GetFieldSignature(klass, id)} = {value}");
#endif
            Storage[GetFieldSignature(klass, id)] = value;
        }
        public HybInstance GetStaticField(Class klass, string id)
        {
#if SS_TRACE
            Console.WriteLine($"get_{GetFieldSignature(klass, id)} => {Storage[GetFieldSignature(klass, id)]}");
#endif
            return Storage[GetFieldSignature(klass, id)];
        }
        private string GetFieldSignature(Class klass, string id)
        {
            return $"{klass.Id}_f_{id}";
        }
    }
}
