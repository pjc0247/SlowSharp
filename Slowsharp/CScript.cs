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

        public static CScript CreateRunner(ScriptConfig scriptConfig = null, RunConfig config = null)
        {
            if (scriptConfig == null)
                scriptConfig = ScriptConfig.Default;
            if (config == null)
                config = RunConfig.Default;

            var r = new Runner(scriptConfig, config);
            return new CScript(r, new SyntaxNode[] { });
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
            var roots = new List<SyntaxNode>();
            foreach (var src in srcs)
            {
                var root = ParseAndValidate(src);
                r.LoadSyntax(root);
                roots.Add(root);
            }

            return new CScript(r, roots.ToArray());
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
        private SyntaxNode[] Roots { get; }

        public CScript(Runner runner, SyntaxNode[] roots)
        {
            this.Runner = runner;
            this.Roots = roots;
        }

        public void Dump()
        {
            foreach (var root in Roots)
                Dump(root, 0);
        }
        public void Dump(SyntaxNode syntax, int depth)
        {
            for (int i = 0; i < depth; i++) Console.Write("  ");
            Console.WriteLine(syntax.GetType() + " " + syntax);

            foreach (var child in syntax.ChildNodes())
                Dump(child, depth + 1);
        }

        public HybInstance Eval(string src)
        {
            var root = ParseAndValidate(src, isScript: true);
            HybInstance ret = null;
            foreach (var child in root.ChildNodes())
            {
                if (child is GlobalStatementSyntax gstmt)
                    ret = Runner.RunScriptStatement(gstmt.Statement);
            }
            return ret;
            //throw new ArgumentException("src is not a expression");
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
        
        public void Trap(MethodInfo target, MethodInfo replace)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (replace == null) throw new ArgumentNullException(nameof(replace));

            Runner.Trap(target, replace);
        }
        public void Untrap(MethodInfo target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            Runner.Untrap(target);
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
