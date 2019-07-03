using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Slowsharp
{
    internal class ExtensionMethodResolver
    {
        private List<MethodInfo> extensions = new List<MethodInfo>();

        public ExtensionMethodResolver(Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
            {
                if (asm.IsDynamic) continue;

                foreach (var type in asm.GetExportedTypes()
                    .Where(x => x.IsSealed && x.IsAbstract))
                {
                    foreach (var method in type.GetMethods()
                        .Where(x => x.IsDefined(typeof(ExtensionAttribute), false)))
                    {
                        extensions.Add(method);
                    }
                }
            }
        }

        public SSMethodInfo[] GetCallablegExtensions(HybInstance value, string id)
        {
            var result = new List<SSMethodInfo>();

            if (value.IsCompiledType)
            {
                foreach (var method in extensions)
                {
                    var first = method.GetParameters()[0]
                        .ParameterType;

                    if (method.Name != id)
                        continue;

                    if (first.IsAssignableFrom(value.GetHybType()))
                        result.Add(new SSCompiledMethodInfo(method));
                }

                return result.ToArray();
            }

            throw new NotImplementedException();
        }
    }
}
