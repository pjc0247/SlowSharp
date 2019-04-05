using System;
using System.Collections;
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
            if (node == null)
                throw new SemanticViolationException("Invalid syntax");

            if (node is ParenthesizedExpressionSyntax ps)
                return RunExpression(ps.Expression);
            else if (node is ParenthesizedExpressionSyntax)
                return RunParenthesized(node as ParenthesizedExpressionSyntax);
            else if (node is ParenthesizedLambdaExpressionSyntax)
                return RunParenthesizedLambda(node as ParenthesizedLambdaExpressionSyntax);
            else if (node is CastExpressionSyntax)
                return RunCast(node as CastExpressionSyntax);
            else if (node is BinaryExpressionSyntax)
                return RunBinaryExpression(node as BinaryExpressionSyntax);
            else if (node is ThisExpressionSyntax)
                return ResolveThis(node as ThisExpressionSyntax);
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

            else if (node is SizeOfExpressionSyntax)
                RunSizeof(node as SizeOfExpressionSyntax);

            else if (node is ObjectCreationExpressionSyntax)
                return RunObjectCreation(node as ObjectCreationExpressionSyntax);
            else if (node is ArrayCreationExpressionSyntax)
                return RunArrayCreation(node as ArrayCreationExpressionSyntax);

            // Runner.ThreadingKeyword.cs
            else if (node is AwaitExpressionSyntax)
                return RunAwait(node as AwaitExpressionSyntax);

            return null;
        }

        private HybInstance ResolveThis(ThisExpressionSyntax node)
        {
            return ctx._this;
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
            {
                if (int.TryParse(node.Token.Text, out _) == false)
                    throw new SemanticViolationException($"Integer literal out of range");
                return HybInstance.Int(i);
            }
            if (node.Token.Value is float f)
                return HybInstance.Float(f);
            if (node.Token.Value is double d)
                return HybInstance.Double(d);

            throw new InvalidOperationException();
        }
        private HybInstance ResolveId(IdentifierNameSyntax node)
        {
            if (string.IsNullOrEmpty(node.Identifier.Text))
                throw new SemanticViolationException($"Invalid syntax: {node.Parent}");

            var id = $"{node.Identifier}";
            HybInstance v = null;

            if (vars.TryGetValue(id, out v))
                return v;

            if (ctx._this != null)
            {
                if (ctx._this.GetPropertyOrField(id, out v, AccessLevel.This))
                    return v;
            }

            if (ctx.method.declaringType.GetStaticPropertyOrField(id, out v))
                return v;

            /*
            Class klass = ctx.method.declaringClass;
            SSFieldInfo field;
            if (klass.TryGetField(id, out field))
            {
                if (field.isStatic)
                    return globals.GetStaticField(klass, id);
            }
            SSPropertyInfo property;
            if (klass.TryGetProperty(id, out property))
            {
                if (property.isStatic)
                    return property.getMethod.Invoke(null, new HybInstance[] { });
            }
            */
            //if (field.)

            throw new NoSuchMemberException($"{id}");
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
                halt = HaltType.None;
            }));
        }

        private HybInstance RunCast(CastExpressionSyntax node)
        {
            var type = resolver.GetType($"{node.Type}");
            var value = RunExpression(node.Expression);

            return value.Cast(type);
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

            var (args, hasRefOrOut) = ResolveArgumentList(node.ArgumentList);

            if (node.Expression is MemberAccessExpressionSyntax ma)
            {
                var leftIsType = false;

                if (ma.Expression is PredefinedTypeSyntax pd)
                {
                    HybType leftType = null;
                    leftIsType = true;
                    leftType = resolver.GetType($"{pd}");
                    callsite = leftType.GetStaticMethods($"{ma.Name}");
                }
                else if (ma.Expression is IdentifierNameSyntax id)
                {
                    HybType leftType = null;
                    if (resolver.TryGetType($"{id.Identifier}", out leftType))
                    {
                        leftIsType = true;
                        callsite = leftType.GetStaticMethods($"{ma.Name}");
                    }
                    else
                    {
                        if (vars.TryGetValue($"{id.Identifier}", out callee))
                        {
                            if (callee == null)
                                throw new NullReferenceException($"{id.Identifier}");
                        }
                        else
                            throw new SemanticViolationException($"Unrecognized identifier: {id.Identifier}");
                        
                        callsite = callee
                            .GetMethods($"{ma.Name}");
                    }

                    calleeId = $"{id.Identifier}";
                }
                else if (ma.Expression is ExpressionSyntax expr)
                {
                    callee = RunExpression(expr);
                    callsite = callee.GetMethods($"{ma.Name}");
                }

                if (leftIsType == false &&
                        callsite.Length == 0)
                {
                    callsite = extResolver.GetCallablegExtensions(callee, $"{ma.Name}");

                    args = (new HybInstance[] { callee }).Concat(args).ToArray();
                }

                targetId = $"{ma.Name}";
                //callsite = ResolveMemberAccess(node.Expression as MemberAccessExpressionSyntax);
            }
            else if (node.Expression is IdentifierNameSyntax id)
            {
                callee = ctx._this;
                callsite =
                    ResolveLocalMember(id)
                    .Concat(ctx.method.declaringType.GetStaticMethods(id.Identifier.Text))
                    .ToArray();
                targetId = id.Identifier.Text;
            }

            if (callsite.Length == 0)
                throw new NoSuchMethodException($"{calleeId}", targetId);
            
            var method = OverloadingResolver.FindMethodWithArguments(
                resolver,
                callsite, ref args);

            if (method == null)
                throw new SemanticViolationException($"No matching override for `{targetId}`");

            if (callee != null && method.declaringType != callee.GetHybType())
                callee = callee.parent;

            var ret = method.target.Invoke(callee, args, hasRefOrOut);

            // post-invoke
            if (hasRefOrOut)
            {
                var count = 0;
                foreach (var arg in node.ArgumentList.Arguments)
                {
                    if (arg.RefKindKeyword != null)
                        RunAssign(arg.Expression, args[count]);
                    count++;
                }
            }

            return ret;
        }
        private (HybInstance[], bool) ResolveArgumentList(ArgumentListSyntax node)
        {
            var args = new HybInstance[node.Arguments.Count];
            var hasRefOrOut = false;

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var refOrOut = node.Arguments[i].RefKindKeyword.Text ?? "";
                if (refOrOut == "ref" || refOrOut == "out")
                    hasRefOrOut = true;
                args[i] = RunExpression(node.Arguments[i].Expression);
            }

            return (args, hasRefOrOut);
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
            if (node.Expression is IdentifierNameSyntax idNode)
            {
                HybType type;
                var id = $"{idNode.Identifier}";
                if (resolver.TryGetType(id, out type))
                    return RunStaticMemberAccess(node, type);
            }

            var left = RunExpression(node.Expression);
            var right = node.Name.Identifier.Text;

            HybInstance o;
            if (left.GetPropertyOrField(right, out o, AccessLevel.Outside))
                return o;

            throw new NoSuchMemberException(right);
        }
        private HybInstance RunStaticMemberAccess(MemberAccessExpressionSyntax node, HybType leftType)
        {
            var right = node.Name.Identifier.Text;

            HybInstance value;
            if (leftType.GetStaticPropertyOrField(right, out value))
                return value;

            throw new SemanticViolationException($"No such static member: {right}");
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

            var inst = type.CreateInstance(this, args);
            if (node.Initializer != null)
                ProcessInitializer(inst, node.Initializer);
            return inst;
        }
        private void ProcessInitializer(HybInstance inst, InitializerExpressionSyntax init)
        {
            if (IsDictionaryAddible(inst, init))
            {
                var setMethod = inst.GetSetIndexerMethod();
                foreach (var expr in init.Expressions)
                {
                    if (expr is AssignmentExpressionSyntax assign)
                    {
                        var right = RunExpression(assign.Right);
                        if (assign.Left is ImplicitElementAccessSyntax ea)
                        {
                            var args = new HybInstance[ea.ArgumentList.Arguments.Count];
                            var count = 0;
                            foreach (var arg in ea.ArgumentList.Arguments)
                                args[count++] = RunExpression(arg.Expression);

                            inst.SetIndexer(args, right);
                        }
                    }
                    else if (expr is InitializerExpressionSyntax initializer)
                    {
                        var left = RunExpression(initializer.Expressions[0]);
                        var right = RunExpression(initializer.Expressions[1]);
                        inst.SetIndexer(new HybInstance[] { left }, right);
                    }
                    else
                        throw new SemanticViolationException("");
                }
            }
            else if (IsArrayAddible(inst))
            {
                var addMethods = inst.GetMethods("Add");
                foreach (var expr in init.Expressions)
                {
                    if (expr is AssignmentExpressionSyntax)
                        throw new SemanticViolationException("");

                    var value = RunExpression(expr);
                    var addArgs = new HybInstance[] { value };
                    var method = OverloadingResolver.FindMethodWithArguments(
                        resolver, addMethods, ref addArgs);

                    method.target.Invoke(inst, addArgs);
                }
            }
        }

        private bool IsArrayAddible(HybInstance obj)
        {
            if (obj.GetMethods("Add").Length == 0)
                return false;
            return typeof(IEnumerable).IsAssignableFrom(obj.GetHybType());
        }
        private bool IsDictionaryAddible(HybInstance obj, InitializerExpressionSyntax init)
        {
            if (init.Expressions.Count > 0 &&
                (init.Expressions[0] is AssignmentExpressionSyntax ||
                 init.Expressions[0] is InitializerExpressionSyntax))
            {
                if (obj.GetSetIndexerMethod() != null)
                    return true;
            }
            return false;
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

            if (node.Initializer != null)
            {
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
            }
            else
            {
                var ranks = node.Type.RankSpecifiers
                    .SelectMany(x => x.Sizes)
                    .Select(x => RunExpression(x).As<int>())
                    .ToArray();
                ary = Array.CreateInstance(elemType, ranks);
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

        private HybInstance RunSizeof(SizeOfExpressionSyntax node)
        {
            var type = $"{node.Type}";
            var size = 0;

            if (type == "byte") size = sizeof(byte);
            else if (type == "sbyte") size = sizeof(sbyte);
            else if (type == "char") size = sizeof(char);
            else if (type == "int") size = sizeof(int);
            else if (type == "uint") size = sizeof(uint);
            else if (type == "short") size = sizeof(short);
            else if (type == "ushort") size = sizeof(ushort);
            else if (type == "long") size = sizeof(long);
            else if (type == "ulong") size = sizeof(ulong);
            else if (type == "float") size = sizeof(float);
            else if (type == "double") size = sizeof(double);
            else if (type == "decimal") size = sizeof(decimal);
            else if (type == "Int16") size = sizeof(Int16);
            else if (type == "Int32") size = sizeof(Int32);
            else if (type == "Int64") size = sizeof(Int64);
            else if (type == "UInt16") size = sizeof(UInt16);
            else if (type == "UInt32") size = sizeof(UInt32);
            else if (type == "UInt64") size = sizeof(UInt64);
            else if (type == "Byte") size = sizeof(Byte);
            else if (type == "SByte") size = sizeof(SByte);
            else if (type == "Double") size = sizeof(Double);
            else if (type == "Decimal") size = sizeof(Decimal);
            else
                throw new SemanticViolationException($"sizeof cannot be used with {type}");

            return HybInstance.Int(size);
        }
    }
}