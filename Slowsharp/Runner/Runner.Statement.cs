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
        private void RunLocalDeclaration(LocalDeclarationStatementSyntax node)
        {
            var typename = $"{node.Declaration.Type}";
            var isVar = typename == "var";
            HybType type = null;

            if (isVar == false)
                type = resolver.GetType(typename);

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
    }
}
