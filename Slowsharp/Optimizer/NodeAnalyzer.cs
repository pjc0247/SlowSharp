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
    internal class NodeAnalyzer
    {
        public static bool IsStaticExpression(ExpressionSyntax node)
        {
            if (node is LiteralExpressionSyntax)
                return true;
            if (node is BinaryExpressionSyntax bin)
                return IsStaticExpression(bin.Left) && IsStaticExpression(bin.Right);
            if (node is DefaultExpressionSyntax)
                return true;
            if (node is SizeOfExpressionSyntax)
                return true;

            return false;
        }

        public bool TrackChanges(SyntaxNode node)
        {
            return false;
        }
    }
}
