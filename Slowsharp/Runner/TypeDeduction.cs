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
            foreach (var child in node.ChildNodes())
            {
                Console.WriteLine(child);
            }
            return null;
        } 

        public static HybType GetType(TypeResolver resolver, CSharpSyntaxNode node)
        {
            if (node is LiteralExpressionSyntax lit)
                return GetTypeLiteral(lit);

            throw new ArgumentException($"Unknown type: {node}");
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
            throw new NotImplementedException();
        }
    }
}
