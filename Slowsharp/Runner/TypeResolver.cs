using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class TypeResolver
    {
        private RunContext ctx;
        private Assembly[] assemblies;

        public TypeResolver(RunContext ctx, Assembly[] assemblies)
        {
            this.ctx = ctx;
            this.assemblies = assemblies;
        }

        private int GetArrayRank(string id)
        {
            if (!(id.Contains("[") && id.Contains("]")))
                return 0;
            return id.Count(x => x == ',') + 1;
        }
        private string GetPureName(string id)
        {
            if (id.Contains("<"))
                return id.Split('<')[0];
            if (id.Contains("["))
                return id.Split('[')[0];
            return id;
        }

        public HybType GetType(string id)
        {
            var pureName = id;
            var rank = GetArrayRank(id);
            if (rank > 0)
                pureName = GetPureName(id);

            var type = _GetType(pureName);
            if (type == null)
                return null;

            var ac = ctx.config.accessControl;
            if (ac.IsSafeType(type) == false)
                throw new SandboxException($"{id} is not allowed to use.");

            if (rank > 0)
                type = type.MakeArrayType(rank);

            return type;
        }
        public HybType _GetType(string id)
        {
            if (id == "int") return new HybType(typeof(int));
            else if (id == "string") return new HybType(typeof(string));
            else if (id == "float") return new HybType(typeof(float));
            else if (id == "double") return new HybType(typeof(double));
            else if (id == "decimal") return new HybType(typeof(decimal));
            else if (id == "uint") return new HybType(typeof(uint));
            else if (id == "object") return new HybType(typeof(object));

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == id)
                        return new HybType(type);
                    if (type.FullName == id)
                        return new HybType(type);
                }
            }

            if (ctx.types.ContainsKey(id))
                return new HybType(ctx.types[id]);

            return null;
        }
        public HybType GetGenericType(string id, int n)
        {
            id = $"{id}`{n}";

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name.Split('[')[0] == id)
                        return new HybType(type);
                    if (type.FullName.Split('[')[0] == id)
                        return new HybType(type);
                }
            }

            return null;
        }
    }
}