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
        private HashSet<HybType> initializedTypes = new HashSet<HybType>();
        private Runner runner;
        private TypeResolver resolver;

        public TypeResolverForStaticInitialization(Runner runner, TypeResolver resolver) :
            base (null, null)
        {
            this.runner = runner;
            this.resolver = resolver;
        }

        public override bool TryGetType(string id, out HybType type, Assembly hintAssembly = null)
        {
            var ret = resolver.TryGetType(id, out type, hintAssembly);
            if (type != null && type.isCompiledType == false)
            {
                if (initializedTypes.Add(type))
                {
                    runner.RunStaticInitializer(type.interpretKlass);
                }
            }
            return ret;
        }
        public override HybType GetType(string id)
        {
            var type = resolver.GetType(id);
            if (type != null && type.isCompiledType == false)
            {
                if (initializedTypes.Add(type))
                {
                    runner.RunStaticInitializer(type.interpretKlass);
                }
            }
            return type;
        }
        public override HybType GetGenericType(string id, int n)
        {
            return resolver.GetGenericType(id, n);
        }
    }
}
