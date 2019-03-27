using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private Assembly asm;

        private RunContext ctx;
        internal Name2RT name2rt { get; }
        private IdLookup lookup;
        private Class klass;
        internal VarFrame vars { get; private set; }
        private Stack<CatchFrame> catches;

        private bool methodEnd;
        private object ret;

        private bool halt;

        public Runner(Assembly asm, RunConfig config)
        {
            this.asm = asm;

            this.ctx = new RunContext(config);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            this.lookup = new IdLookup(assemblies);
            this.catches = new Stack<CatchFrame>();
            this.name2rt = new Name2RT(ctx, assemblies);
            //RunMethod(klass.GetMethods("Main")[0]);
        }

        public void RunMain()
        {
            klass.GetMethods("Main")[0].Invoke(null, new object[] { });
        }

        public void Run(SyntaxNode node)
        {
            var treatAsBlock = new Type[]
            {
                typeof(CompilationUnitSyntax),
                typeof(NamespaceDeclarationSyntax),
                typeof(ClassDeclarationSyntax)
            };

            if (node is UsingDirectiveSyntax)
                AddUsing(node as UsingDirectiveSyntax);
            if (node is ClassDeclarationSyntax)
                AddClass(node as ClassDeclarationSyntax);
            if (node is ConstructorDeclarationSyntax)
                RunConstructorDeclaration(node as ConstructorDeclarationSyntax);
            if (node is FieldDeclarationSyntax)
                AddField(node as FieldDeclarationSyntax);
            if (node is MethodDeclarationSyntax)
                RunMethodDeclaration(node as MethodDeclarationSyntax);
            if (node is BlockSyntax)
                RunBlock(node as BlockSyntax);
            if (node is IfStatementSyntax)
                RunIf(node as IfStatementSyntax);
            if (node is ForStatementSyntax)
                RunFor(node as ForStatementSyntax);
            if (node is TryStatementSyntax)
                RunTry(node as TryStatementSyntax);
            if (node is ReturnStatementSyntax)
                RunReturn(node as ReturnStatementSyntax);
            if (node is LocalDeclarationStatementSyntax)
                RunLocalDeclaration(node as LocalDeclarationStatementSyntax);
            if (node is VariableDeclarationSyntax)
                RunVariableDeclaration(node as VariableDeclarationSyntax);
            if (node is ExpressionStatementSyntax)
                RunExpressionStatement(node as ExpressionStatementSyntax);

            if (treatAsBlock.Contains(node.GetType()))
                RunChildren(node);

            if (halt) return;
        }

        private void RunChildren(SyntaxNode node)
        {
            foreach (var child in node.ChildNodes())
                Run(child);
        }

        private void AddUsing(UsingDirectiveSyntax node)
        {
            lookup.Add($"{node.Name}");
        }
        private void AddClass(ClassDeclarationSyntax node)
        {
            klass = new Class(this);
            ctx.types.Add($"{node.Identifier}", klass);
        }
        private void AddField(FieldDeclarationSyntax node)
        {
            foreach (var f in node.Declaration.Variables) 
                klass.AddField($"{f.Identifier}", node);
        }

        internal void RunBlock(BlockSyntax node, VarFrame vf)
        {
            vars = vf;

            foreach (var child in node.ChildNodes())
            {
                try
                {
                    Run(child);
                }
                catch (Exception e) when (catches.Count > 0)
                {
                    Console.WriteLine(e);

                    foreach (var c in catches.Reverse())
                    {
                        if (c.RunCatch(e))
                            break;
                    }
                }

                if (ctx.IsExpird())
                    halt = true;

                if (halt || methodEnd) break;
            }
            vars = vars.parent;
        }
        internal void RunBlock(BlockSyntax node)
        {
            RunBlock(node, new VarFrame(vars));
        }
        private void RunTry(TryStatementSyntax node)
        {
            catches.Push(new CatchFrame(this, node));
            RunBlock(node.Block);
            catches.Pop();
        }

        private void RunConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            klass.AddMethod("$_ctor", node);
        }
        private void RunMethodDeclaration(MethodDeclarationSyntax node)
        {
            klass.AddMethod(node.Identifier.ValueText, node);
        }
        internal object RunMethod(BaseMethodDeclarationSyntax node)
        {
            ret = null; methodEnd = false;
            RunBlock(node.Body);
            return ret;
        }
        internal object RunMethod(HybInstance _this, BaseMethodDeclarationSyntax node)
        {
            ctx._this = _this;
            return RunMethod(node);
        }
        private void RunReturn(ReturnStatementSyntax node)
        {
            Console.WriteLine(node.Expression);
            ret = RunExpression(node.Expression);
            methodEnd = true;
            Console.WriteLine($"Return {ret}");
        }
        private void RunLocalDeclaration(LocalDeclarationStatementSyntax node)
        {
            foreach (var v in node.Declaration.Variables)
            {
                vars.SetValue(v.Identifier.ValueText, RunExpression(v.Initializer.Value));
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
        
        private void RunAssign(AssignmentExpressionSyntax node)
        {
            var right = RunExpression(node.Right);

            if (node.Left is IdentifierNameSyntax id)
            {
                var key = id.Identifier.ValueText;
                var set = false;
                if (ctx._this != null)
                {
                    if (ctx._this.SetPropertyOrField(key, right))
                        set = true;
                }

                if (set == false)
                    vars.SetValue(key, right);
            }
        }
        private object RunInvocation(InvocationExpressionSyntax node)
        {
            object callee = null;
            Invokable[] callsite = null;

            if (node.Expression is MemberAccessExpressionSyntax ma)
            {
                if (ma.Expression is IdentifierNameSyntax id)
                {
                    var leftType = name2rt.GetType($"{id.Identifier}");
                    if (leftType == null)
                    {
                        callee = vars.GetValue($"{id.Identifier}");

                        if (callee == null)
                            throw new NullReferenceException($"{id.Identifier}");

                        callsite = callee.GetType()
                            .GetMember($"{ma.Name}")
                            .OfType<MethodInfo>()
                            .Select(x => new Invokable(x))
                            .ToArray();
                    }
                    else
                    {
                        callsite = leftType.GetMethods($"{ma.Name}");
                    }
                }
                //callsite = ResolveMemberAccess(node.Expression as MemberAccessExpressionSyntax);
            }
            if (node.Expression is IdentifierNameSyntax)
                callsite = ResolveLocalMember(node.Expression as IdentifierNameSyntax);

            if (callsite == null)
                return null;

            var args = ResolveArgumentList(node.ArgumentList);
            var method = FindMethodWithArguments(callsite, args);
            var ret = method.Invoke(callee, args);
            methodEnd = false;
            return ret;
        }

        private object[] ResolveArgumentList(ArgumentListSyntax node)
        {
            var args = new object[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
                args[i] = RunExpression(node.Arguments[i].Expression);

            return args;
        }

        private object ResolveLiteral(LiteralExpressionSyntax node)
        {
            return node.Token.Value;
            /*
            if (node.Token.Kind() == SyntaxKind.StringLiteralExpression)
                return node.Token.Value;
            return null;
            */
        }

        private Invokable[] ResolveLocalMember(IdentifierNameSyntax node)
        {
            var id = node.Identifier.ValueText;
            return klass.GetMethods(id);
        }
        private Invokable[] ResolveMemberAccess(MemberAccessExpressionSyntax node)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == $"{node.Expression}")
                    {
                        return type.GetMember($"{node.Name}")
                            .Where(x => x is MethodInfo)
                            .Select(x => new Invokable(x as MethodInfo))
                            .ToArray();
                    }
                }
            }

            return new Invokable[] { };
        }
        private Invokable FindMethodWithArguments(Invokable[] members, object[] args)
        {
            foreach (var member in members)
            {
                if (member.isCompiled)
                {
                    var method = member.compiledMethod;
                    var ps = method.GetParameters();

                    if (args.Length != ps.Length)
                        continue;

                    bool skip = false;
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (args[i] == null)
                        {
                            if (ps[i].ParameterType.IsValueType)
                            {
                                skip = true;
                                break;
                            }
                            continue;
                        }
                        if (!ps[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip) continue;

                    return member;
                }
                else
                {
                    var ps = member.interpretMethod.ParameterList.Parameters;

                    if (args.Length != ps.Count)
                        continue;

                    foreach (var p in ps)
                    {
                        Console.WriteLine(p.Type);
                        //p.Type
                    }

                    return member;
                }
            }

            return null;
        }
    }
}
