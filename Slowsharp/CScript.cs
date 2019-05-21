using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class CScript
    {
        public static object RunSimple(string src, ScriptConfig scriptConfig = null, RunConfig config = null)
        {
            return Run(@"
using System;

class CScript__ {
public static object Main() {
    return " + src + @";
}
}
", scriptConfig, config);
        }
        public static object Run(string src, ScriptConfig scriptConfig = null, RunConfig config = null)
        {
            var r = CreateRunner(src, scriptConfig, config);

            var ret = r.RunMain();
            if (ret == null) return null;
            return ret.Unwrap();
        }

        public static CScript CreateRunner(string[] srcs, ScriptConfig scriptConfig = null, RunConfig config = null)
        {
            if (srcs == null || srcs.Length == 0)
                throw new ArgumentException(nameof(srcs));

            if (scriptConfig == null)
                scriptConfig = ScriptConfig.Default;
            if (config == null)
                config = RunConfig.Default;

            var r = new Runner(scriptConfig, config);
            foreach (var src in srcs)
            {
                var root = ParseAndValidate(src);
                r.LoadSyntax(root);
            }

            return new CScript(r);
        }
        public static CScript CreateRunner(string src, ScriptConfig scriptConfig = null, RunConfig config = null)
            => CreateRunner(new string[] { src }, scriptConfig, config);

        private static CSharpSyntaxNode ParseAndValidate(string src, bool isScript = false)
        {
            if (string.IsNullOrEmpty(src))
                throw new ArgumentException("src is null or empty string");

            var options = new CSharpParseOptions(
                LanguageVersion.Default,
                kind: isScript ? SourceCodeKind.Script : SourceCodeKind.Regular);
            var tree = CSharpSyntaxTree.ParseText(src, options);
            var root = tree.GetCompilationUnitRoot();

            var vd = new Validator();
            vd.Visit(root);

            return root;
        }

        private Runner Runner { get; }

        public CScript(Runner runner)
        {
            this.Runner = runner;
        }

        public HybInstance Eval(string src)
        {
            var root = ParseAndValidate(src, isScript: true);

            var glob = root.ChildNodes().First() as GlobalStatementSyntax;
            if (glob.Statement is ExpressionStatementSyntax expr) {
                return Runner.RunExpression(expr.Expression);
            }

            throw new ArgumentException("src is not a expression");
        }
        public void LoadScript(string src)
        {
            var root = ParseAndValidate(src);
            Runner.LoadSyntax(root);
        }
        public void UpdateMethodsOnly(string src)
        {
            var root = ParseAndValidate(src);
            Runner.UpdateMethodsOnly(root);
        }

        public HybInstance RunMain(params object[] args)
            => Runner.RunMain(args);
        public HybInstance Instantiate(string id, params object[] args)
            => Runner.Instantiate(id, args);
        public HybInstance Override(string id, object parentObject, params object[] args)
            => Runner.Override(id, parentObject, args);

        public DumpSnapshot GetDebuggerDump()
            => Runner.GetDebuggerDump();
        public HybType[] GetTypes()
            => Runner.GetTypes();
    }
}
