using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        internal int RunBlock(BlockSyntax node, VarFrame vf, int pc = 0)
        {
            var prevVars = vars;
            vars = vf;

            var children = node.ChildNodes().ToArray();

            if (children.Length == pc)
                return -1;

            for (; pc < children.Length; pc++)
            {
                var child = children[pc];

                try
                {
                    Run(child);
                }
                catch (Exception e) when (catches.Count > 0)
                {
                    Console.WriteLine(e);

                    foreach (var c in catches.Reverse())
                    {
                        if (c.RunCatch(e))
                            break;
                    }
                }

                if (ctx.IsExpird())
                    throw new TimeoutException();

                if (halt != HaltType.None)
                {
                    pc++;
                    break;
                }
            }

            vars = prevVars;

            return pc;
        }
        internal int RunBlock(BlockSyntax node)
        {
            return RunBlock(node, new VarFrame(vars));
        }

        private void RunLocalDeclaration(LocalDeclarationStatementSyntax node)
        {
            var cache = optCache.GetOrCreate(node, () => {
                var _typename = $"{node.Declaration.Type}";
                var _isVar = _typename == "var";

                return new OptLocalDeclarationNode() {
                    isVar = _isVar,
                    type = _isVar ? null : resolver.GetType(_typename)
                };
            });

            var isVar = cache.isVar;
            var type = cache.type;

            foreach (var v in node.Declaration.Variables)
            {
                var id = v.Identifier.ValueText;
                if (vars.TryGetValue(id, out _))
                    throw new SemanticViolationException($"Local variable redefination: {id}");
                if (isVar && v.Initializer == null)
                    throw new SemanticViolationException($"`var` should be initialized with declaration.");

                HybInstance value = null;
                if (v.Initializer != null)
                    value = RunExpression(v.Initializer.Value);
                else
                    value = type.GetDefault();
                vars.SetValue(id, value);
            }
        }
        private void RunVariableDeclaration(VariableDeclarationSyntax node)
        {
            foreach (var v in node.Variables)
            {
                vars.SetValue(v.Identifier.ValueText, RunExpression(v.Initializer.Value));
            }
        }
        private void RunExpressionStatement(ExpressionStatementSyntax node)
        {
            RunExpression(node.Expression);
        }

        private void RunLabeled(LabeledStatementSyntax node)
        {
            Run(node.Statement);
        }
        private void RunThrow(ThrowStatementSyntax node)
        {
            var ex = RunExpression(node.Expression);

            if (ex.isCompiledType)
            {
                if (ex.innerObject is Exception e)
                    throw e;
                throw new SemanticViolationException($"Exception must be derived from System.Exception."); ;
            }
            throw new WrappedException(ex);
        }
        private void RunReturn(ReturnStatementSyntax node)
        {
            if (node.Expression != null)
                ret = RunExpression(node.Expression);

            halt = HaltType.Return;
        }
        private void RunYield(YieldStatementSyntax node)
        {
            if (node.Expression != null)
                ret = RunExpression(node.Expression);

            // yield break;
            if (node.ReturnOrBreakKeyword.Text == "break")
                halt = HaltType.YieldBreak;
            // yield return EXPR;
            else
                halt = HaltType.YieldReturn;
        }
    }
}
