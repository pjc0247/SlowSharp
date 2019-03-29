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
        internal HybInstance RunExpression(ExpressionSyntax node)
        {
            if (node is ParenthesizedExpressionSyntax ps)
                return RunExpression(ps.Expression);
            else if (node is ParenthesizedExpressionSyntax)
                return RunParenthesized(node as ParenthesizedExpressionSyntax);
            else if (node is ParenthesizedLambdaExpressionSyntax)
                return RunParenthesizedLambda(node as ParenthesizedLambdaExpressionSyntax);
            else if (node is BinaryExpressionSyntax)
                return RunBinaryExpression(node as BinaryExpressionSyntax);
            else if (node is LiteralExpressionSyntax)
                return ResolveLiteral(node as LiteralExpressionSyntax);
            else if (node is ElementAccessExpressionSyntax)
                return RunElementAccess(node as ElementAccessExpressionSyntax);
            else if (node is MemberAccessExpressionSyntax)
                return RunMemberAccess(node as MemberAccessExpressionSyntax);
            else if (node is AssignmentExpressionSyntax)
                RunAssign(node as AssignmentExpressionSyntax);
            else if (node is DefaultExpressionSyntax)
                return RunDefault(node as DefaultExpressionSyntax);
            else if (node is InterpolatedStringExpressionSyntax)
                return RunInterpolatedString(node as InterpolatedStringExpressionSyntax);
            else if (node is InvocationExpressionSyntax)
                return RunInvocation(node as InvocationExpressionSyntax);
            else if (node is ConditionalExpressionSyntax)
                return RunConditional(node as ConditionalExpressionSyntax);
            else if (node is IdentifierNameSyntax)
                return ResolveId(node as IdentifierNameSyntax);
            else if (node is PostfixUnaryExpressionSyntax)
                return RunPostfixUnary(node as PostfixUnaryExpressionSyntax);

            else if (node is ObjectCreationExpressionSyntax)
                return RunObjectCreation(node as ObjectCreationExpressionSyntax);
            else if (node is ArrayCreationExpressionSyntax)
                return RunArrayCreation(node as ArrayCreationExpressionSyntax);

            // Runner.ThreadingKeyword.cs
            else if (node is AwaitExpressionSyntax)
                return RunExpression(node as AwaitExpressionSyntax);

            return null;
        }

        private HybInstance ResolveLiteral(LiteralExpressionSyntax node)
        {
            if (node.Token.Value == null)
                return HybInstance.Null();
            if (node.Token.Value is char c)
                return HybInstance.Char(c);
            if (node.Token.Value is string str)
                return HybInstance.String(str);
            if (node.Token.Value is bool b)
                return HybInstance.Bool(b);
            if (node.Token.Value is int i)
                return HybInstance.Int(i);
            if (node.Token.Value is float f)
                return HybInstance.Float(f);
            if (node.Token.Value is double d)
                return HybInstance.Double(d);

            throw new InvalidOperationException();
        }
        private HybInstance ResolveId(IdentifierNameSyntax node)
        {
            var id = $"{node.Identifier}";
            HybInstance v = null;

            if (vars.TryGetValue(id, out v))
                return v;

            if (ctx._this != null)
            {
                if (ctx._this.GetPropertyOrField(id, out v, AccessLevel.Outside))
                    return v;
            }

            return null;
        }

        private HybInstance RunParenthesized(ParenthesizedExpressionSyntax node)
        {
            return RunExpression(node.Expression);
        }
        private HybInstance RunParenthesizedLambda(ParenthesizedLambdaExpressionSyntax node)
        {
            return new HybInstance(new HybType(typeof(Action)), new Action(() =>
            {
                Run(node.Body);
            }));
        }

        private HybInstance RunDefault(DefaultExpressionSyntax node)
        {
            var type = resolver.GetType($"{node.Type}");
            return type.GetDefault();
        }
        private HybInstance RunConditional(ConditionalExpressionSyntax node)
        {
            var cond = RunExpression(node.Condition);
            if (IsTrueOrEquivalent(cond))
                return RunExpression(node.WhenTrue);
            else
                return RunExpression(node.WhenFalse);
        }

        private HybInstance RunInterpolatedString(InterpolatedStringExpressionSyntax node)
        {
            var sb = new StringBuilder();

            foreach (var content in node.Contents)
            {
                if (content is InterpolationSyntax s)
                    sb.Append(RunExpression(s.Expression));
                else if (content is InterpolatedStringTextSyntax)
                    sb.Append(content.GetText());
            }

            return HybInstance.String(sb.ToString());
        }

        private HybInstance RunInvocation(InvocationExpressionSyntax node)
        {
            string calleeId = "";
            string targetId = "";
            HybInstance callee = null;
            SSMethodInfo[] callsite = null;

            if (node.Expression is MemberAccessExpressionSyntax ma)
            {
                if (ma.Expression is IdentifierNameSyntax id)
                {
                    var leftType = resolver.GetType($"{id.Identifier}");
                    if (leftType == null)
                    {
                        callee = vars.GetValue($"{id.Identifier}");

                        if (callee == null)
                            throw new NullReferenceException($"{id.Identifier}");

                        callsite = callee
                            .GetMethods($"{ma.Name}");
                    }
                    else
                    {
                        callsite = leftType.GetStaticMethods($"{ma.Name}");
                    }

                    calleeId = $"{id.Identifier}";
                }
                else if (ma.Expression is ExpressionSyntax expr)
                {
                    callee = RunExpression(expr);
                    callsite = callee.GetMethods($"{ma.Name}");
                }

                targetId = $"{ma.Name}";
                //callsite = ResolveMemberAccess(node.Expression as MemberAccessExpressionSyntax);
            }
            else if (node.Expression is IdentifierNameSyntax id)
            {
                callsite = ResolveLocalMember(node.Expression as IdentifierNameSyntax);
                targetId = id.Identifier.Text;
            }

            if (callsite.Length == 0)
                throw new NoSuchMethodException($"{calleeId}", targetId);

            var args = ResolveArgumentList(node.ArgumentList);
            var method = FindMethodWithArguments(callsite, args);

            if (method == null)
                throw new SemanticViolationException($"No matching override for `{targetId}`");

            var ret = method.target.Invoke(callee, args);
            return ret;
        }

        private HybInstance RunElementAccess(ElementAccessExpressionSyntax node)
        {
            var left = RunExpression(node.Expression);
            var args = new HybInstance[node.ArgumentList.Arguments.Count];

            var count = 0;
            foreach (var arg in node.ArgumentList.Arguments)
                args[count++] = RunExpression(arg.Expression);

            HybInstance o;
            if (left.GetIndexer(args, out o))
                return o;

            throw new NoSuchMemberException("[]");
        }
        private HybInstance RunMemberAccess(MemberAccessExpressionSyntax node)
        {
            var left = RunExpression(node.Expression);
            var right = node.Name.Identifier.Text;

            HybInstance o;
            if (left.GetPropertyOrField(right, out o, AccessLevel.Outside))
                return o;

            throw new NoSuchMemberException(right);
        }
        private HybInstance RunObjectCreation(ObjectCreationExpressionSyntax node)
        {
            Console.WriteLine("CreateObject");

            HybType type = null;

            var args = new HybInstance[node.ArgumentList.Arguments.Count];
            var count = 0;
            foreach (var arg in node.ArgumentList.Arguments)
                args[count++] = RunExpression(arg.Expression);

            if (node.Type is GenericNameSyntax gn)
            {
                type = resolver.GetGenericType(
                    $"{gn.Identifier}",
                    gn.TypeArgumentList.Arguments.Count);

                var genericArgs = new HybType[gn.TypeArgumentList.Arguments.Count];
                count = 0;
                foreach (var arg in gn.TypeArgumentList.Arguments)
                    genericArgs[count++] = resolver.GetType($"{arg}");
                type = type.MakeGenericType(genericArgs);
            }
            else
                type = resolver.GetType($"{node.Type}");

            if (type.isCompiledType)
            {
                if (type.compiledType == typeof(Action))
                    return args[0];
                if (type.compiledType == typeof(Func<int>))
                    return args[0];
            }

            return type.CreateInstance(this, args);
        }
        private HybInstance RunArrayCreation(ArrayCreationExpressionSyntax node)
        {
            var typeId = $"{node.Type.ElementType}";
            var rtAry = resolver.GetType(typeId);

            Array ary = null;
            Type elemType;
            if (rtAry.isCompiledType)
                elemType = rtAry.compiledType;
            else
                elemType = typeof(HybInstance);

            ary = Array.CreateInstance(
                elemType, node.Initializer.Expressions.Count);

            var count = 0;
            foreach (var expr in node.Initializer.Expressions)
            {
                var value = RunExpression(expr);

                if (rtAry.isCompiledType)
                    ary.SetValue(value.innerObject, count++);
                else
                    ary.SetValue(value, count++);
            }

            return HybInstance.Object(ary);
        }

        private HybInstance RunPostfixUnary(PostfixUnaryExpressionSyntax node)
        {
            var delta = node.OperatorToken.Text == "++" ? 1 : -1;

            if (node.Operand is IdentifierNameSyntax id)
            {
                var v = vars.GetValue($"{id.Identifier}");
                var after = v + delta;
                vars.SetValue($"{id.Identifier}", after);
                return after;
            }

            return null;
        }
    }
}