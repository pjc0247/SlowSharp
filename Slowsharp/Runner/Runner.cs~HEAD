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

        internal RunContext ctx;
        internal ExtensionMethodResolver extResolver { get; }
        internal TypeResolver resolver { get; }
        private IdLookup lookup;
        private Class klass;
        internal VarFrame vars { get; private set; }
        private Stack<CatchFrame> catches;

        private Stack<VarFrame> frames { get; }

        internal HybInstance ret;
        private HaltType halt;

        public Runner(Assembly asm, RunConfig config)
        {
            this.asm = asm;

            this.ctx = new RunContext(config);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            this.lookup = new IdLookup(assemblies);
            this.catches = new Stack<CatchFrame>();
            this.frames = new Stack<VarFrame>();
            this.extResolver = new ExtensionMethodResolver(assemblies);
            this.resolver = new TypeResolver(ctx, assemblies);
        }

        internal void BindThis(HybInstance _this)
        {
            ctx._this = _this;
        }

        internal HybInstance RunMain(params object[] args)
        {
            ctx.Reset();
            return klass.GetMethods("Main")[0]
                .target.Invoke(null, args.Wrap());
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
                AddConstructorMethod(node as ConstructorDeclarationSyntax);
            if (node is PropertyDeclarationSyntax)
                AddProperty(node as PropertyDeclarationSyntax);
            if (node is FieldDeclarationSyntax)
                AddField(node as FieldDeclarationSyntax);
            if (node is MethodDeclarationSyntax)
                AddMethod(node as MethodDeclarationSyntax);
            if (node is BlockSyntax)
                RunBlock(node as BlockSyntax);
            if (node is ArrowExpressionClauseSyntax)
                RunArrowExpressionClause(node as ArrowExpressionClauseSyntax);
            if (node is ThrowStatementSyntax)
                RunThrow(node as ThrowStatementSyntax);
            if (node is GotoStatementSyntax)
                RunGoto(node as GotoStatementSyntax);
            if (node is IfStatementSyntax)
                RunIf(node as IfStatementSyntax);
            if (node is ForStatementSyntax)
                RunFor(node as ForStatementSyntax);
            if (node is ForEachStatementSyntax)
                RunForEach(node as ForEachStatementSyntax);
            if (node is TryStatementSyntax)
                RunTry(node as TryStatementSyntax);
            if (node is ReturnStatementSyntax)
                RunReturn(node as ReturnStatementSyntax);
            if (node is BreakStatementSyntax)
                RunBreak(node as BreakStatementSyntax);
            if (node is ContinueStatementSyntax)
                RunContinue(node as ContinueStatementSyntax);
            if (node is LocalDeclarationStatementSyntax)
                RunLocalDeclaration(node as LocalDeclarationStatementSyntax);
            if (node is LabeledStatementSyntax)
                RunLabeled(node as LabeledStatementSyntax);
            if (node is VariableDeclarationSyntax)
                RunVariableDeclaration(node as VariableDeclarationSyntax);
            if (node is ExpressionStatementSyntax)
                RunExpressionStatement(node as ExpressionStatementSyntax);

            if (node is LockStatementSyntax)
                RunLock(node as LockStatementSyntax);

            if (treatAsBlock.Contains(node.GetType()))
                RunChildren(node);

            if (ctx.IsExpird())
                throw new TimeoutException();
        }

        private void RunChildren(SyntaxNode node)
        {
            foreach (var child in node.ChildNodes())
                Run(child);
        }

        private HybInstance RunArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            return RunExpression(node.Expression);
        }
        private HybInstance RunArrowExpressionClause(ArrowExpressionClauseSyntax node, VarFrame vf)
        {
            vars = vf;
            var ret = RunExpression(node.Expression);
            vars = vars.parent;
            return ret;
        }
        internal void RunBlock(BlockSyntax node, VarFrame vf, int pc = 0)
        {
            vars = vf;

            var children = node.ChildNodes().ToArray();
            for (int i = pc; i < children.Length; i++)
            {
                var child = children[i];

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
                    throw new TimeoutException();

                if (halt != HaltType.None) break;
            }
            vars = vars.parent;
        }
        internal void RunBlock(BlockSyntax node)
        {
            RunBlock(node, new VarFrame(vars));
        }

        internal HybInstance RunMethod(SSMethodInfo method, HybInstance[] args)
        {
            ret = null;
            ctx.PushMethod(method);

            var node = method.declaration;
            var vf = new VarFrame(null);
            var count = 0;
            foreach (var arg in args)
            {
                var p = node.ParameterList.Parameters[count++];
                var paramId = p.Identifier.Text;

                if (p.Modifiers.IsParams())
                    break;

                vf.SetValue(paramId, arg);
            }

            if (method.isVaArg)
            {
                var paramId = node.ParameterList.Parameters.Last()
                    .Identifier.Text;

                var vaArgs = args.Skip(count - 1).ToArray();
                vf.SetValue(paramId, HybInstance.ObjectArray(vaArgs));
            }

            frames.Push(vars);
            vars = null;

            if (node.Body != null)
                RunBlock(node.Body, vf);
            else
                ret = RunArrowExpressionClause(node.ExpressionBody, vf);

            vars = frames.Pop();

            if (halt == HaltType.Return)
                halt = HaltType.None;

            return ret;
        }
        internal HybInstance RunMethod(HybInstance _this, SSMethodInfo method, HybInstance[] args)
        {
            ctx._this = _this;
            return RunMethod(method, args);
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
        
        private void UpdateVariable(string key, HybInstance value)
        {
            if (vars.UpdateValue(key, value) == false)
            {
                if (ctx._this != null)
                {
                    if (ctx._this.SetPropertyOrField(key, value, AccessLevel.Outside))
                        ;
                }
            }
        }

        private void RunAssign(AssignmentExpressionSyntax node)
        {
            // +=, -=, *=, /=
            if (IsOpAndAssignToken(node.OperatorToken))
            {
                RunAssignWithOp(node);
                return;
            }

            var right = RunExpression(node.Right);

            if (node.Left is IdentifierNameSyntax id)
            {
                var key = id.Identifier.ValueText;
                var set = false;
                if (ctx._this != null)
                {
                    if (ctx._this.SetPropertyOrField(key, right, AccessLevel.Outside))
                        set = true;
                }

                if (set == false)
                    vars.SetValue(key, right);
            }
            else if (node.Left is MemberAccessExpressionSyntax ma)
            {
                var left = RunExpression(ma.Expression);
                left.SetPropertyOrField($"{ma.Name}", right, AccessLevel.Outside);
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
        private void RunAssignWithOp(AssignmentExpressionSyntax node)
        {
            var right = RunExpression(node.Right);

            if (node.Left is IdentifierNameSyntax id)
            {
                var key = id.Identifier.ValueText;
                var value = MadMath.Op(ResolveId(id), right, node.OperatorToken.Text.Substring(0, 1));

                UpdateVariable(key, value);
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

        private HybInstance[] ResolveArgumentList(ArgumentListSyntax node)
        {
            var args = new HybInstance[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
                args[i] = RunExpression(node.Arguments[i].Expression);

            return args;
        }

        private SSMethodInfo[] ResolveLocalMember(IdentifierNameSyntax node)
        {
            var id = node.Identifier.ValueText;
            return klass.GetMethods(id);
        }
    }
}
