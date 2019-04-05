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

        private TypeCache typeCache;

        public TypeResolver(RunContext ctx, Assembly[] assemblies)
        {
            this.ctx = ctx;
            this.assemblies = assemblies;

            this.typeCache = new TypeCache(ctx, assemblies);
        }

        public void AddLookupNamespace(string ns)
        {
            typeCache.AddLookupNamespace(ns);
        }

        private bool IsGeneric(string id)
        {
            return id.Count(x => x == '<') != 0;
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
        /// <summary>
        /// Retrives signature name which is compatible with .Net
        /// </summary>
        private string GetSignatureName(string id, out string[] genericArgs)
        {
            if (id.Contains("<"))
            {
                int depth = 0;
                var count = 0;
                var args = new List<string>();
                var offset = 0;

                for (int i = 0; i < id.Length; i++)
                {
                    if (id[i] == '<')
                    {
                        depth++;
                        if (depth == 1)
                            offset = i + 1;
                    }
                    if (id[i] == '>')
                    {
                        depth--;
                        if (depth == 0)
                            args.Add(id.Substring(offset, i - offset).Trim());
                    }

                    if (depth == 1 && id[i] == ',')
                    {
                        count++;
                        args.Add(id.Substring(offset, i - offset).Trim());
                        offset = i + 1;
                    }
                }

                genericArgs = args.ToArray();
                return $"{GetPureName(id)}`{count + 1}";
            }

            genericArgs = null;
            return id;
        }

        public bool TryGetType(string id, out HybType type)
        {
            var sig = GetPureName(id);
            var rank = GetArrayRank(id);
            var isGeneric = IsGeneric(id);
            string[] genericArgs = null;

            if (isGeneric)
                sig = GetSignatureName(id, out genericArgs);

            type = typeCache.GetType(sig);
            if (type == null)
                return false;    

            var ac = ctx.config.accessControl;
            if (ac.IsSafeType(type) == false)
                throw new SandboxException($"{id} is not allowed to use.");

            if (isGeneric)
            {
                var genericArgTypes = new HybType[genericArgs.Length];
                for (int i = 0; i < genericArgTypes.Length; i++)
                    genericArgTypes[i] = GetType(genericArgs[i]);
                type = type.MakeGenericType(genericArgTypes);
            }
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
        
        public HybType GetGenericType(string id, int n)
        {
            id = $"{id}`{n}";

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypesSafe())
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