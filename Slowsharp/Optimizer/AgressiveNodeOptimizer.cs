using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class AgressiveNodeOptimizer
    {
        private Runner runner;

        public AgressiveNodeOptimizer(Runner runner)
        {
            this.runner = runner;
        }

        public bool IsOptimizable()
        {
            return false;
        }
        public void Process(SyntaxNode node)
        {
            var parent = node.Parent;

            if (node is ArgumentListSyntax argList)
                ProcessArgumentList(argList);
        }

        private void ProcessArgumentList(ArgumentListSyntax node)
        {
            foreach (var arg in node.Arguments)
            {
                if (NodeAnalyzer.IsStaticExpression(arg.Expression) == false)
                    return;
            }

            var values = new HybInstance[node.Arguments.Count];
            var count = 0;
            foreach (var arg in node.Arguments)
            {
                values[count++] = runner.RunExpression(arg.Expression);
            }

            /*
            new EvaluatedArgumentListSyntax()
            {
                arguments = values
            };
            */
        }
    }
}
