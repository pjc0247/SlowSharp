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
        internal object RunExpression(ExpressionSyntax node)
        {
            if (node is BinaryExpressionSyntax)
                return RunBinaryExpression(node as BinaryExpressionSyntax);
            if (node is LiteralExpressionSyntax)
                return ResolveLiteral(node as LiteralExpressionSyntax);
            else if (node is MemberAccessExpressionSyntax)
                return RunMemberAccess(node as MemberAccessExpressionSyntax);
            else if (node is AssignmentExpressionSyntax)
                RunAssign(node as AssignmentExpressionSyntax);
            else if (node is InvocationExpressionSyntax)
                return RunInvocation(node as InvocationExpressionSyntax);
            else if (node is IdentifierNameSyntax)
                return ResolveId(node as IdentifierNameSyntax);
            else if (node is PostfixUnaryExpressionSyntax)
                return RunPostfixUnary(node as PostfixUnaryExpressionSyntax);

            else if (node is ObjectCreationExpressionSyntax)
                return RunObjectCreation(node as ObjectCreationExpressionSyntax);
            else if (node is ArrayCreationExpressionSyntax)
                return RunArrayCreation(node as ArrayCreationExpressionSyntax);

            return null;
        }

        private object ResolveId(IdentifierNameSyntax node)
        {
            var id = $"{node.Identifier}";
            object v = null;

            if (vars.TryGetValue(id, out v))
                return v;

            if (ctx._this != null)
            {
                if (ctx._this.GetPropertyOrField(id, out v))
                    return v;
            }

            return null;
        }

        private object RunMemberAccess(MemberAccessExpressionSyntax node)
        {
            var left = RunExpression(node.Expression);
            var right = node.Name.Identifier.Text;

            return left.GetType().GetMember(right)
                .FirstOrDefault()
                .GetValue(left);
        }
        private object RunObjectCreation(ObjectCreationExpressionSyntax node)
        {
            HybType type = null;

            var args = new object[node.ArgumentList.Arguments.Count];
            var count = 0;
            foreach (var arg in node.ArgumentList.Arguments)
                args[count++] = RunExpression(arg.Expression);

            if (node.Type is GenericNameSyntax gn)
            {
                /*
                type = name2rt.GetGenericType(
                    $"{gn.Identifier}",
                    gn.TypeArgumentList.Arguments.Count);

                var genericArgs = new Type[gn.TypeArgumentList.Arguments.Count];
                var count = 0;
                foreach (var arg in gn.TypeArgumentList.Arguments)
                    genericArgs[count++] = name2rt.GetType($"{arg}");
                type = type.MakeGenericType(genericArgs);
                */
            }
            else
                type = name2rt.GetType($"{node.Type}");

            return type.CreateInstance(this, args);
        }
        private object RunArrayCreation(ArrayCreationExpressionSyntax node)
        {
            var typeId = $"{node.Type.ElementType}";
            var rtAry = name2rt.GetType(typeId);

            /*
            var ary = Array.CreateInstance(
                rtAry, node.Initializer.Expressions.Count);
            var count = 0;
            foreach (var expr in node.Initializer.Expressions)
                ary.SetValue(RunExpression(expr), count ++);

            return ary;
            */
            return null;
        }

        private object RunPostfixUnary(PostfixUnaryExpressionSyntax node)
        {
            var delta = node.OperatorToken.Text == "++" ? 1 : -1;

            if (node.Operand is IdentifierNameSyntax id)
            {
                var v = vars.GetValue($"{id.Identifier}");
                var after = Convert.ChangeType(Convert.ToDecimal(v) + delta, v.GetType());

                vars.SetValue($"{id.Identifier}", after);

                return v;
            }

            return null;
        }
    }
}
