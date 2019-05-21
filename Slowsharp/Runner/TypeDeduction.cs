using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class TypeDeduction
    {
        public static HybType GetReturnType(TypeResolver resolver, CSharpSyntaxNode node)
        {
            var candidates = new List<HybType>();

            // Collect
            foreach (var child in node.ChildNodes()
                .OfType<ReturnStatementSyntax>())
            {
                var type = GetType(resolver, child.Expression);
                if (type != null)
                    candidates.Add(type);
            }

            // Deduct
            if (candidates.Count == 0)
                return HybTypeCache.Void;

            var finalCandidate = candidates[0];
            foreach (var candidate in candidates)
            {
                if (candidate.IsCompiledType == false)
                    finalCandidate = HybTypeCache.Object;
            }

            return finalCandidate;
        } 

        public static HybType GetType(TypeResolver resolver, ExpressionSyntax node)
        {
            if (node is LiteralExpressionSyntax lit)
                return GetTypeLiteral(lit);
            else if (node is BinaryExpressionSyntax bin)
                return GetTypeBinaryOperation(bin);

            return null;
        }
        private static HybType GetTypeBinaryOperation(BinaryExpressionSyntax node)
        {
            var op = node.OperatorToken.Text;
            if (op == "==" || op == "!=" ||
                op == ">"  || op == ">=" ||
                op == "<"  || op == "<=")
                return HybTypeCache.Bool;

            return null;
        }
        private static HybType GetTypeLiteral(LiteralExpressionSyntax node)
        {
            if (node.Token.Value is char c)
                return HybType.Char;
            if (node.Token.Value is string str)
                return HybType.String;
            if (node.Token.Value is bool b)
                return HybType.Bool;
            if (node.Token.Value is int i)
                return HybType.Int32;
            if (node.Token.Value is float f)
                return HybType.Float;
            if (node.Token.Value is double d)
                return HybType.Double;

            throw new ArgumentException($"Unknown type: {node.Token}");
        }
        private static HybType GetTypeInvocation(InvocationExpressionSyntax node)
        {
            return null;
        }
    }
}
