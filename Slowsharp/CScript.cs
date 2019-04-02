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
        public static object RunSimple(string src, RunConfig config = null)
        {
            return Run(@"
using System;

class CScript__ {
public static object Main() {
    return " + src + @";
}
}
", config);
        }
        public static object Run(string src, RunConfig config = null)
        {
            if (config == null)
                config = RunConfig.Default;

            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            var r = new Runner(config);
            r.Run(root);

            var ret = r.RunMain();
            if (ret == null) return null;
            return ret.Unwrap();
        }
    }
}
