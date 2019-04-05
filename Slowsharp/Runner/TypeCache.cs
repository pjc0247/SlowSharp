using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class TypeCache
    {
        private Dictionary<string, HybType> cache = new Dictionary<string, HybType>();
        private RunContext ctx;
        private Assembly[] assemblies;

        private List<string> namespaces = new List<string>();

        public TypeCache(RunContext ctx, Assembly[] assemblies)
        {
            this.ctx = ctx;
            this.assemblies = assemblies;
        }

        public void AddLookupNamespace(string ns)
        {
            namespaces.Add(ns);
        }

        public HybType GetType(string id)
        {
            if (cache.ContainsKey(id))
                return cache[id];

            var type = FindType(id);
            cache[id] = type;
            return type;
        }
        private HybType FindType(string id)
        {
            if (id == "void") return HybType.Void;
            else if (id == "int") return HybType.Int32;
            else if (id == "char") return HybType.Char;
            else if (id == "byte") return HybType.Byte;
            else if (id == "sbyte") return HybType.Sbyte;
            else if (id == "bool") return HybType.Bool;
            else if (id == "short") return HybType.Short;
            else if (id == "ushort") return HybType.Ushort;
            else if (id == "string") return HybType.String;
            else if (id == "float") return HybType.Float;
            else if (id == "double") return HybType.Double;
            else if (id == "decimal") return HybType.Decimal;
            else if (id == "uint") return HybType.Uint32;
            else if (id == "object") return HybType.Object;

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypesSafe())
                {
                    if (type.Name == id &&
                        namespaces.Contains(type.Namespace))
                        return new HybType(type);
                    if (type.FullName == id)
                        return new HybType(type);
                }
            }

            if (ctx.types.ContainsKey(id))
                return new HybType(ctx.types[id]);

            return null;
        }
    }
}
