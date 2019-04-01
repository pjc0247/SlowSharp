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

        public bool TryGetType(string id, out HybType type)
        {
            var pureName = id;
            var rank = GetArrayRank(id);
            if (rank > 0)
                pureName = GetPureName(id);

            type = _GetType(pureName);
            if (type == null)
                return false;    

            var ac = ctx.config.accessControl;
            if (ac.IsSafeType(type) == false)
                throw new SandboxException($"{id} is not allowed to use.");

            if (rank > 0)
                type = type.MakeArrayType(rank);

            return true;
        }
        public HybType GetType(string id)
        {
            HybType type;
            if (TryGetType(id, out type))
                return type;

            throw new SemanticViolationException($"Unrecognized type {id}");
        }
        public HybType _GetType(string id)
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