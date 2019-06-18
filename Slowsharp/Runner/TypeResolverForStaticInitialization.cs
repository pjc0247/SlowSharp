using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class TypeResolverForStaticInitialization : TypeResolver
    {
        private HashSet<HybType> InitializedTypes = new HashSet<HybType>();
        private Runner Runner;
        private TypeResolver Resolver;

        public TypeResolverForStaticInitialization(Runner runner, TypeResolver resolver) :
            base (null, null)
        {
            this.Runner = runner;
            this.Resolver = resolver;
        }

        public override bool TryGetType(string id, out HybType type, Assembly hintAssembly = null)
        {
            var ret = Resolver.TryGetType(id, out type, hintAssembly);
            if (type != null && type.IsCompiledType == false)
            {
                if (InitializedTypes.Add(type))
                {
                    Runner.RunStaticInitializer(type.InterpretKlass);
                }
            }
            return ret;
        }
        public override HybType GetType(string id)
        {
            var type = Resolver.GetType(id);
            if (type != null && type.IsCompiledType == false)
            {
                if (InitializedTypes.Add(type))
                {
                    Runner.RunStaticInitializer(type.InterpretKlass);
                }
            }
            return type;
        }
        public override HybType GetGenericType(string id, int n)
        {
            return Resolver.GetGenericType(id, n);
        }
    }
}
