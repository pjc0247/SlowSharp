using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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
            else if (node is ConditionalAccessExpressionSyntax)
                return RunConditionalAccess(node as ConditionalAccessExpressionSyntax);
            else if (node is IdentifierNameSyntax)
                return ResolveId(node as IdentifierNameSyntax);
            else if (node is PrefixUnaryExpressionSyntax)
                return RunPrefixUnary(node as PrefixUnaryExpressionSyntax);
            else if (node is PostfixUnaryExpressionSyntax)
                return RunPostfixUnary(node as PostfixUnaryExpressionSyntax);

            else if (node is TypeOfExpressionSyntax)
                return RunTypeof(node as TypeOfExpressionSyntax);
            else if (node is SizeOfExpressionSyntax)
                return RunSizeof(node as SizeOfExpressionSyntax);

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
            return Ctx._this;
        }
        private HybInstance ResolveLiteral(LiteralExpressionSyntax node)
        {
            var cache = OptCache.GetOrCreate(node, () => {
                var optNode = new OptLiteralNode();

                if (node.Token.Value == null)
                    optNode.value = HybInstance.Null();
                else if (node.Token.Value is char c)
                    optNode.value = HybInstance.Char(c);
                else if (node.Token.Value is string str)
                    optNode.value = HybInstance.String(str);
                else if (node.Token.Value is bool b)
                    optNode.value = HybInstance.Bool(b);
                else if (node.Token.Value is int i)
                {
                    if (int.TryParse(node.Token.Text, out _) == false)
                        throw new SemanticViolationException($"Integer literal out of range");
                    optNode.value = HybInstance.Int(i);
                }
                else if (node.Token.Value is float f)
                    optNode.value = HybInstance.Float(f);
                else if (node.Token.Value is double d)
                    optNode.value = HybInstance.Double(d);
                else
                    throw new InvalidOperationException();

                return optNode;
            });

            return cache.value;
        }
        private HybInstance ResolveId(IdentifierNameSyntax node)
        {
            if (string.IsNullOrEmpty(node.Identifier.Text))
                throw new SemanticViolationException($"Invalid syntax: {node.Parent}");

            var id = $"{node.Identifier}";
            HybInstance v = null;

            if (Vars.TryGetValue(id, out v))
                return v;

            if (Ctx._this != null)
            {
                if (Ctx._this.GetPropertyOrField(id, out v, AccessLevel.This))
                    return v;
            }

            if (Ctx.Method.DeclaringType.GetStaticPropertyOrField(id, out v))
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

        private void RunAssign(AssignmentExpressionSyntax node)
        {
            // +=, -=, *=, /=
            if (IsOpAndAssignToken(node.OperatorToken))
            {
                RunAssignWithOp(node);
                return;
            }

            RunAssign(node.Left, RunExpression(node.Right));
        }
        private void RunAssign(ExpressionSyntax leftNode, HybInstance right)
        {
            if (leftNode is IdentifierNameSyntax id)
            {
                var key = id.Identifier.ValueText;
                UpdateVariable(key, right);
            }
            else if (leftNode is MemberAccessExpressionSyntax ma)
            {
                if (ma.Expression is IdentifierNameSyntax idNode)
                {
                    var key = $"{idNode.Identifier}";
                    HybType leftType;
                    if (Resolver.TryGetType(key, out leftType))
                    {
                        leftType.SetStaticPropertyOrField($"{ma.Name.Identifier}", right);
                        return;
                    }
                }

                var left = RunExpression(ma.Expression);
                var accessLevel = AccessLevel.Outside;

                if (ma.Expression is ThisExpressionSyntax)
                    accessLevel = AccessLevel.This;

                left.SetPropertyOrField($"{ma.Name}", right, accessLevel);
            }
            else if (leftNode is ElementAccessExpressionSyntax ea)
            {
                var callee = RunExpression(ea.Expression);
                var args = new HybInstance[ea.ArgumentList.Arguments.Count];

                var count = 0;
                foreach (var arg in ea.ArgumentList.Arguments)
                    args[count++] = RunExpression(arg.Expression);

                if (callee.SetIndexer(args, right) == false)
                    throw new NoSuchMemberException("[]");
            }
        }
        private void RunAssignWithOp(AssignmentExpressionSyntax node)
        {
            var right = RunExpression(node.Right);

            if (node.Left is IdentifierNameSyntax id)
            {
                var key = id.Identifier.ValueText;
                var value = MadMath.Op(ResolveId(id), right, node.OperatorToken.Text.Substring(0, 1));

                UpdateVariable(key, value);
            }
            else if (node.Left is MemberAccessExpressionSyntax ma)
            {
                HybInstance left = null;
                HybInstance value = null;

                if (ma.Expression is IdentifierNameSyntax idNode)
                {
                    var key = $"{idNode.Identifier}";
                    HybType leftType;
                    if (Resolver.TryGetType(key, out leftType))
                    {
                        leftType.GetStaticPropertyOrField($"{ma.Name.Identifier}", out left);
                        value = MadMath.Op(left, right, node.OperatorToken.Text.Substring(0, 1));
                        leftType.SetStaticPropertyOrField($"{ma.Name.Identifier}", value);
                        return;
                    }
                }

                var callee = RunExpression(ma.Expression);
                var accessLevel = AccessLevel.Outside;

                if (ma.Expression is ThisExpressionSyntax)
                    accessLevel = AccessLevel.This;

                callee.GetPropertyOrField($"{ma.Name}", out left, accessLevel);
                value = MadMath.Op(left, right, node.OperatorToken.Text.Substring(0, 1));
                callee.SetPropertyOrField($"{ma.Name}", value, accessLevel);
            }
            else if (node.Left is ElementAccessExpressionSyntax ea)
            {
                var callee = RunExpression(ea.Expression);
                var args = new HybInstance[ea.ArgumentList.Arguments.Count];

                var count = 0;
                foreach (var arg in ea.ArgumentList.Arguments)
                    args[count++] = RunExpression(arg.Expression);

                HybInstance value;
                callee.GetIndexer(args, out value);

                value = MadMath.Op(value, right, node.OperatorToken.Text.Substring(0, 1));

                if (callee.SetIndexer(args, value) == false)
                    throw new NoSuchMemberException("[]");
            }
        }
        private bool IsOpAndAssignToken(SyntaxToken token)
        {
            if (token.Text.Length == 2 &&
                token.Text[1] == '=' && token.Text[0] != '=')
                return true;
            return false;
        }

        /// <summary>
        /// Runs parenthesized expression.
        ///   [Syntax] () => Math.Max(1, 5)
        /// </summary>
        private HybInstance RunParenthesized(ParenthesizedExpressionSyntax node)
        {
            return RunExpression(node.Expression);
        }

        /// <summary>
        /// Runs cast expression.
        ///   [Syntax] (int)b
        /// </summary>
        private HybInstance RunCast(CastExpressionSyntax node)
        {
            var cache = OptCache.GetOrCreate(node, () => {
                return new OptCastNode() {
                    type = Resolver.GetType($"{node.Type}")
                };
            });
            var value = RunExpression(node.Expression);

            return value.Cast(cache.type);
        }

        /// <summary>
        /// Runs default expression.
        ///   [Syntax] default(int)
        /// </summary>
        private HybInstance RunDefault(DefaultExpressionSyntax node)
        {
            var type = Resolver.GetType($"{node.Type}");
            return type.GetDefault();
        }
        /// <summary>
        /// Runs conditional expression.
        ///   [Syntax] CONDITION ? IF_TRUE : IF_FALSE
        /// </summary>
        private HybInstance RunConditional(ConditionalExpressionSyntax node)
        {
            var cond = RunExpression(node.Condition);
            if (IsTrueOrEquivalent(cond))
                return RunExpression(node.WhenTrue);
            else
                return RunExpression(node.WhenFalse);
        }
        /// <summary>
        /// Runs conditional access expression. (CS6)
        ///   [Syntax] EXPR?.WHEN_NOT_NULL
        /// </summary>
        private HybInstance RunConditionalAccess(ConditionalAccessExpressionSyntax node)
        {
            var left = RunExpression(node.Expression);

            Ctx._bound = left;

            if (left.IsNull() == false)
                return RunExpression(node.WhenNotNull);
            return HybInstance.Null();
        }

        /// <summary>
        /// Runs interpolated string expression.
        ///   [Syntax] $"My name is {VALUE}"
        /// </summary>
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

        /// <summary>
        /// Runs invocation expression.
        ///   [Syntax] Console.WriteLine("Hello World");
        ///            Foo(1234);
        /// </summary>
        private HybInstance RunInvocation(InvocationExpressionSyntax node)
        {
            string calleeId = "";
            string targetId = "";
            HybInstance callee = null;
            SSMethodInfo[] callsite = null;
            HybType[] implicitGenericArgs = null;

            var (args, hasRefOrOut) = ResolveArgumentList(node.ArgumentList);

            if (node.Expression is MemberAccessExpressionSyntax ma)
            {
                var leftIsType = false;
                var rightName = $"{ma.Name.Identifier}";

                implicitGenericArgs = ResolveGenericArgumentsFromName(ma.Name);

                if (ma.Expression is PredefinedTypeSyntax pd)
                {
                    HybType leftType = null;
                    leftIsType = true;
                    leftType = Resolver.GetType($"{pd}");
                    callsite = leftType.GetStaticMethods(rightName);
                }
                else if (ma.Expression is IdentifierNameSyntax id)
                {
                    HybType leftType = null;
                    if (Resolver.TryGetType($"{id.Identifier}", out leftType))
                    {
                        leftIsType = true;
                        callsite = leftType.GetStaticMethods(rightName);
                    }
                    else
                    {
                        callee = ResolveId(id);
                        callsite = callee.GetMethods(rightName);
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
                    callsite = ExtResolver.GetCallablegExtensions(callee, $"{ma.Name}");

                    args = (new HybInstance[] { callee }).Concat(args).ToArray();
                }

                targetId = $"{ma.Name}";
                //callsite = ResolveMemberAccess(node.Expression as MemberAccessExpressionSyntax);
            }
            else if (node.Expression is SimpleNameSyntax ||
                node.Expression is MemberBindingExpressionSyntax)
            {
                SimpleNameSyntax id = node.Expression as SimpleNameSyntax;
                if (id == null)
                {
                    id = (node.Expression as MemberBindingExpressionSyntax)?.Name;
                    callee = Ctx._bound;
                }
                else
                    callee = Ctx._this;

                implicitGenericArgs = ResolveGenericArgumentsFromName(id);
                callsite =
                    ResolveLocalMember(id)
                    .Concat(Ctx.Method.DeclaringType.GetStaticMethods(id.Identifier.Text))
                    .ToArray();
                targetId = id.Identifier.Text;
            }

            if (callsite.Length == 0)
                throw new NoSuchMethodException($"{calleeId}", targetId);
            
            var method = OverloadingResolver.FindMethodWithArguments(
                Resolver,
                callsite, 
                implicitGenericArgs.ToArray(),
                ref args);

            if (method == null)
                throw new SemanticViolationException($"No matching override for `{targetId}`");

            if (callee != null && method.DeclaringType.Parent == callee.GetHybType())
                callee = callee.Parent;

            var ret = method.Target.Invoke(callee, args, hasRefOrOut);

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
        private HybType[] ResolveGenericArgumentsFromName(SimpleNameSyntax name)
        {
            if (name is GenericNameSyntax gn)
            {
                var result = new HybType[gn.TypeArgumentList.Arguments.Count];
                var count = 0;
                foreach (var genericType in gn.TypeArgumentList.Arguments)
                    result[count++] = Resolver.GetType($"{genericType}");
                return result;
            }
            return new HybType[] { };
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
        private SSMethodInfo[] ResolveLocalMember(SimpleNameSyntax node)
        {
            var id = node.Identifier.ValueText;
            return Ctx.Method.DeclaringClass.GetMethods(id);
        }

        private HybInstance RunNameOf(SimpleNameSyntax node)
        {
            return HybInstance.String(node.Identifier.Text.Split('.').Last());
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
            var cache = OptCache.GetOrCreate<MemberAccessExpressionSyntax, OptRunMemberAccessNode>(node,
                () => {
                    var result = new OptRunMemberAccessNode();

                    if (node.Expression is IdentifierNameSyntax idNode)
                    {
                        HybType type;
                        var id = $"{idNode.Identifier}";
                        if (Resolver.TryGetType(id, out type))
                        {
                            result.leftType = type;
                            result.isStaticMemberAccess = true;
                        }
                    }

                    return result;
                });
            if (cache.isStaticMemberAccess)
                return RunStaticMemberAccess(node, cache.leftType);

            var left = RunExpression(node.Expression);
            var right = node.Name.Identifier.Text;

            var accessLevel = AccessLevel.Outside;
            if (node.Expression is ThisExpressionSyntax ||
                left.GetHybType() == Ctx.Method.DeclaringType)
            {
                // TODO: protected
                accessLevel = AccessLevel.This;
            }

            HybInstance o;
            if (left.GetPropertyOrField(right, out o, accessLevel))
                return o;

            throw new NoSuchMemberException(right);
        }
        private HybInstance RunStaticMemberAccess(MemberAccessExpressionSyntax node, HybType leftType)
        {
            var right = node.Name.Identifier.Text;

            var accessLevel = AccessLevel.Outside;
            if (Ctx.Method.DeclaringType == leftType)
                accessLevel = AccessLevel.This;

            HybInstance value;
            if (leftType.GetStaticPropertyOrField(right, out value, accessLevel))
                return value;

            throw new SemanticViolationException($"No such static member: {right}");
        }

        private HybInstance RunObjectCreation(ObjectCreationExpressionSyntax node)
        {
            HybType type = null;

            var args = new HybInstance[node.ArgumentList.Arguments.Count];
            var count = 0;
            foreach (var arg in node.ArgumentList.Arguments)
                args[count++] = RunExpression(arg.Expression);

            if (node.Type is GenericNameSyntax gn)
            {
                type = Resolver.GetGenericType(
                    $"{gn.Identifier}",
                    gn.TypeArgumentList.Arguments.Count);

                var genericArgs = new HybType[gn.TypeArgumentList.Arguments.Count];
                count = 0;
                foreach (var arg in gn.TypeArgumentList.Arguments)
                    genericArgs[count++] = Resolver.GetType($"{arg}");
                type = type.MakeGenericType(genericArgs);
            }
            else
                type = Resolver.GetType($"{node.Type}");

            if (type.IsCompiledType)
            {
                if (type.CompiledType == typeof(Action))
                    return args[0];
                if (type.CompiledType == typeof(Func<int>))
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
                        Resolver, addMethods, new HybType[] { }, ref addArgs);

                    method.Target.Invoke(inst, addArgs);
                }
            }
            else
            {
                foreach (var expr in init.Expressions)
                {
                    if (expr is AssignmentExpressionSyntax assign)
                    {
                        var id = (IdentifierNameSyntax)assign.Left;
                        var value = RunExpression(assign.Right);
                        if (inst.SetPropertyOrField($"{id.Identifier}", value) == false)
                            throw new SemanticViolationException($"No such member: {id}");
                    }
                    else
                        throw new SemanticViolationException("");
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
            var rtAry = Resolver.GetType($"{node.Type.ElementType}");

            Array ary = null;
            Type elemType;
            if (rtAry.IsCompiledType)
                elemType = rtAry.CompiledType;
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

                    if (rtAry.IsCompiledType)
                        ary.SetValue(value.InnerObject, count++);
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

        private HybInstance RunPrefixUnary(PrefixUnaryExpressionSyntax node)
        {
            var op = node.OperatorToken.Text;
            var operand = RunExpression(node.Operand);
            var cache = OptCache.GetOrCreate<PrefixUnaryExpressionSyntax, OptPrefixUnary>(node, () =>
            {
                return new OptPrefixUnary()
                {
                    operandId = (node.Operand is IdentifierNameSyntax id) ?
                        $"{id.Identifier}" : null,
                    isPrimitiveIncOrDec = 
                        (op == "++" || op == "--") &&
                        node.Operand is IdentifierNameSyntax &&
                        operand.GetHybType().IsPrimitive
                };
            });

            var after = MadMath.PrefixUnary(operand, op);

            if (cache.isPrimitiveIncOrDec)
            {
                var applied = MadMath.Op(
                    operand,
                    HybInstanceCache.One,
                    op.Substring(1));
                Vars.SetValue(cache.operandId, applied);
            }

            return after;
        }
        private HybInstance RunPostfixUnary(PostfixUnaryExpressionSyntax node)
        {
            var op = node.OperatorToken.Text;
            var operand = RunExpression(node.Operand);
            var cache = OptCache.GetOrCreate<PostfixUnaryExpressionSyntax, OptPostfixUnary>(node, () =>
            {
                return new OptPostfixUnary()
                {
                    operandId = (node.Operand is IdentifierNameSyntax id) ?
                        $"{id.Identifier}" : null,
                    isPrimitiveIncOrDec =
                        (op == "++" || op == "--") &&
                        node.Operand is IdentifierNameSyntax &&
                        operand.GetHybType().IsPrimitive
                };
            });

            var after = MadMath.PostfixUnary(operand, op);

            if (cache.isPrimitiveIncOrDec)
            {
                var applied = MadMath.Op(
                    operand,
                    HybInstanceCache.One,
                    op.Substring(1));
                Vars.SetValue(cache.operandId, applied);
            }

            return operand;
        }

        private HybInstance RunTypeof(TypeOfExpressionSyntax node)
        {
            var cache = OptCache.GetOrCreate<TypeOfExpressionSyntax, OptTypeofNode>(node,
                () =>
                {
                    var type = Resolver.GetType($"{node.Type}");
                    return new OptTypeofNode()
                    {
                        type = type
                    };
                });
            
            if (cache.type.IsCompiledType)
                return HybInstance.FromType(cache.type.CompiledType);
            else
                return HybInstance.FromType(typeof(HybType));
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