using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;

namespace Slowsharp
{
    public class CScript
    {
        public static object Run(string src)
        {
            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            var r = new Runner(Assembly.GetEntryAssembly(), new RunConfig());
            r.Run(root);

            var ret = r.RunMain();
            if (ret == null) return null;
            if (ret.isCompiledType)
                return ret.innerObject;
            return ret;
        }
    }
}
