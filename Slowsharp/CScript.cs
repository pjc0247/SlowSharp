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
            var r = CreateRunner(src, config);

            var ret = r.RunMain();
            if (ret == null) return null;
            return ret.Unwrap();
        }
        public static CScript CreateRunner(string src, RunConfig config = null)
        {
            if (config == null)
                config = RunConfig.Default;

            var root = ParseAndValidate(src);
            var r = new Runner(config);
            r.LoadSyntax(root);

            return new CScript(r);
        }
        private static CSharpSyntaxNode ParseAndValidate(string src)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentException("src is null or empty string");

            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            var vd = new Validator();
            vd.Visit(root);

            return root;
        }

        private Runner runner { get; }

        public CScript(Runner runner)
        {
            this.runner = runner;
        }

        public void UpdateMethodsOnly(string src)
        {
            var root = ParseAndValidate(src);
            runner.UpdateMethodsOnly(root);
        }

        public HybInstance RunMain(params object[] args)
            => runner.RunMain(args);
        public HybInstance Instantiate(string id, params object[] args)
            => runner.Instantiate(id, args);
        public HybInstance Override(string id, object parentObject, params object[] args)
            => runner.Override(id, parentObject, args);
    }
}
