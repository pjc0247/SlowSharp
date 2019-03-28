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
        internal TypeResolver resolver { get; }
        private IdLookup lookup;
        private Class klass;
        internal VarFrame vars { get; private set; }
        private Stack<CatchFrame> catches;

        private Stack<VarFrame> frames { get; }

        private bool methodEnd;
        private HybInstance ret;

        private bool halt;

        public Runner(Assembly asm, RunConfig config)
        {
            this.asm = asm;

            this.ctx = new RunContext(config);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            this.lookup = new IdLookup(assemblies);
            this.catches = new Stack<CatchFrame>();
            this.frames = new Stack<VarFrame>();
            this.resolver = new TypeResolver(ctx, assemblies);
            //RunMethod(klass.GetMethods("Main")[0]);
        }

        internal HybInstance RunMain(params object[] args)
        {
            ctx.Reset();
            return klass.GetMethods("Main")[0].Invoke(null, args.Wrap());
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

            if (ctx.IsExpird())
                halt = true;
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
            {
                klass.AddField($"{f.Identifier}", node, f);
            }
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
        internal HybInstance RunMethod(BaseMethodDeclarationSyntax node, HybInstance[] args)
        {
            ret = null; methodEnd = false;

            var vf = new VarFrame(null);
            var count = 0;
            foreach (var arg in args)
            {
                var paramId = node.ParameterList.Parameters[count++].Identifier.Text;
                vf.SetValue(paramId, arg);
            }

            frames.Push(vars);
            vars = null;
            RunBlock(node.Body, vf);
            vars = frames.Pop();

            return ret;
        }
        internal HybInstance RunMethod(HybInstance _this, BaseMethodDeclarationSyntax node, HybInstance[] args)
        {
            ctx._this = _this;
            return RunMethod(node, args);
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
                var id = v.Identifier.ValueText;
                if (vars.TryGetValue(id, out _))
                    throw new SemanticViolationException($"Local variable redefination: {id}");
                vars.SetValue(id, RunExpression(v.Initializer.Value));
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
            else if (node.Left is ElementAccessExpressionSyntax ea)
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

        private HybInstance[] ResolveArgumentList(ArgumentListSyntax node)
        {
            var args = new HybInstance[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
                args[i] = RunExpression(node.Arguments[i].Expression);

            return args;
        }

        private HybInstance ResolveLiteral(LiteralExpressionSyntax node)
        {
            if (node.Token.Value is string str)
                return HybInstance.String(str);
            if (node.Token.Value is bool b)
                return HybInstance.Bool(b);
            if (node.Token.Value is int i)
                return HybInstance.Int(i);
            if (node.Token.Value is float f)
                return HybInstance.Float(f);

            throw new InvalidOperationException();
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
        private Invokable FindMethodWithArguments(Invokable[] members, HybInstance[] args)
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
                        if (!ps[i].ParameterType.IsAssignableFrom(args[i].GetHybType().compiledType))
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
