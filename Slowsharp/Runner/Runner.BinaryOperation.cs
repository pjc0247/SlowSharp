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
        private HybInstance RunBinaryExpression(BinaryExpressionSyntax node)
        {
            var op = node.OperatorToken.ValueText;
            if (op == "is")
                return RunIs(node);

            var left = RunExpression(node.Left);

            if (op == "||") {
                if (IsTrueOrEquivalent(left))
                    return HybInstance.Bool(true);
            }
            else if (op == "&&")
            {
                if (IsTrueOrEquivalent(left) == false)
                    return HybInstance.Bool(false);
            }

            var right = RunExpression(node.Right);

            return MadMath.Op(left, right, op);
        }

        private HybInstance RunIs(BinaryExpressionSyntax node)
        {
            var left = RunExpression(node.Left);
            var type = Resolver.GetType($"{node.Right}");

            if (type == null)
                throw new SemanticViolationException($"Unrecognized type: {node.Right}");

            return left.Is(type) ? HybInstance.Bool(true) : HybInstance.Bool(false);
        }
    }
}
