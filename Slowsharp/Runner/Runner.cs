using System;
using System.Collections;
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
        internal ScriptConfig scriptConfig { get; }
        internal RunContext ctx;
        internal GlobalStorage globals { get; }
        internal ExtensionMethodResolver extResolver { get; }
        internal TypeResolver resolver { get; }
        private IdLookup lookup;
        private Class klass;
        internal VarFrame vars { get; private set; }
        private Stack<CatchFrame> catches;
        private OptNodeCache optCache;

        private Stack<VarFrame> frames { get; }

        internal HybInstance ret;
        private HaltType halt;

        private RunMode runMode;

        public Runner(ScriptConfig scriptConfig, RunConfig config)
        {
            this.ctx = new RunContext(config);
            this.scriptConfig = scriptConfig;
            this.globals = new GlobalStorage();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            this.optCache = new OptNodeCache();
            this.lookup = new IdLookup(assemblies);
            this.catches = new Stack<CatchFrame>();
            this.frames = new Stack<VarFrame>();
            this.extResolver = new ExtensionMethodResolver(assemblies);
            this.resolver = new TypeResolver(ctx, assemblies);

            AddDefaultUsings();
            PrewarmTypes();
        }
        private void AddDefaultUsings()
        {
            foreach (var ns in scriptConfig.DefaultUsings)
                resolver.AddLookupNamespace(ns);
        }
        private void PrewarmTypes()
        {
            foreach (var type in scriptConfig.PrewarmTypes)
                resolver.CacheType(type);
        }

        public HybType[] GetTypes()
        {
            return ctx.types
                .Select(x => x.Value)
                .Select(x => new HybType(x))
                .ToArray();
        }

        public DumpSnapshot GetDebuggerDump()
        {
            return new DumpSnapshot(this);
        }
        public void EndDebug()
        {
        }

        public void LoadSyntax(SyntaxNode node)
        {
            runMode = RunMode.Preparse;
            Run(node);
            runMode = RunMode.Execution;
        }
        public void UpdateMethodsOnly(SyntaxNode node)
        {
            runMode = RunMode.HotLoadMethodsOnly;
            Run(node);
            runMode = RunMode.Execution;
        }

        internal void BindThis(HybInstance _this)
        {
            ctx._this = _this;
        }

        internal HybInstance RunMain(params object[] args)
        {
            ctx.Reset();
            RunLazyInitializers();

            foreach (var type in ctx.types)
            {
                var mains = type.Value.GetMethods("Main");
                if (mains.Length == 0)
                    continue;

                return mains[0].target.Invoke(null, args.Wrap());
            }

            throw new InvalidOperationException($"No class found that contains static Main()");
        }

        /// <summary>
        /// Instantiates an object with typename and args.
        /// </summary>
        /// <param name="id">Typename to instantiate</param>
        /// <param name="args">Arguments passed to the constructor</param>
        /// <returns>HybInstance object</returns>
        public HybInstance Instantiate(string id, params object[] args)
        {
            RunLazyInitializers();
            return resolver.GetType(id).CreateInstance(this, args.Wrap());
        }
        /// <summary>
        /// Overrides an object with already instantiated object.
        /// This performs `virtual inherit` which links two objects.
        /// See documentation for more information.
        /// </summary>
        /// <param name="id">Typename to override</param>
        /// <param name="parentObject">Parent object</param>
        /// <param name="args">Arguments passed to the constructor</param>
        /// <returns>HybInstance object</returns>
        public HybInstance Override(string id, object parentObject, params object[] args)
        {
            RunLazyInitializers();
            return resolver.GetType(id).Override(this, args.Wrap(), parentObject);
        }

        public void Run(SyntaxNode node)
        {
            ctx.lastNode = node;

            var treatAsBlock = new Type[]
            {
                typeof(CompilationUnitSyntax),
                typeof(NamespaceDeclarationSyntax),
                typeof(ClassDeclarationSyntax)
            };

            switch (runMode)
            {
                case RunMode.Preparse:
                    RunAsPreparse(node);
                    break;
                case RunMode.HotLoadMethodsOnly:
                    RunAsHotReloadMethodsOnly(node);
                    break;
                case RunMode.Execution:
                    RunAsExecution(node);
                    break;
            }

            if (treatAsBlock.Contains(node.GetType()))
                RunChildren(node);

            if (ctx.IsExpird())
                throw new TimeoutException();
        }
        public void RunAsPreparse(SyntaxNode node)
        {
            if (node is UsingDirectiveSyntax)
                AddUsing(node as UsingDirectiveSyntax);
            else if (node is ClassDeclarationSyntax)
                AddClass(node as ClassDeclarationSyntax);
            else if (node is ConstructorDeclarationSyntax)
                AddConstructorMethod(node as ConstructorDeclarationSyntax);
            else if (node is PropertyDeclarationSyntax)
                AddProperty(node as PropertyDeclarationSyntax);
            else if (node is FieldDeclarationSyntax)
                AddField(node as FieldDeclarationSyntax);
            else if (node is MethodDeclarationSyntax)
                AddMethod(node as MethodDeclarationSyntax);
        }
        public void RunAsHotReloadMethodsOnly(SyntaxNode node)
        {
            if (node is UsingDirectiveSyntax)
                AddUsing(node as UsingDirectiveSyntax);
            else if (node is ClassDeclarationSyntax classDeclaration)
                klass = ctx.types[classDeclaration.Identifier.Text];
            else if (node is MethodDeclarationSyntax)
                AddMethod(node as MethodDeclarationSyntax);
        }
        public void RunAsExecution(SyntaxNode node)
        {
            if (node is BlockSyntax)
                RunBlock(node as BlockSyntax);
            else if (node is ArrowExpressionClauseSyntax)
                RunArrowExpressionClause(node as ArrowExpressionClauseSyntax);
            else if (node is ThrowStatementSyntax)
                RunThrow(node as ThrowStatementSyntax);
            else if (node is GotoStatementSyntax)
                RunGoto(node as GotoStatementSyntax);
            else if (node is IfStatementSyntax)
                RunIf(node as IfStatementSyntax);
            else if (node is ForStatementSyntax)
                RunFor(node as ForStatementSyntax);
            else if (node is SwitchStatementSyntax)
                RunSwitch(node as SwitchStatementSyntax);
            else if (node is ForEachStatementSyntax)
                RunForEach(node as ForEachStatementSyntax);
            else if (node is WhileStatementSyntax)
                RunWhile(node as WhileStatementSyntax);
            else if (node is TryStatementSyntax)
                RunTry(node as TryStatementSyntax);
            else if (node is ReturnStatementSyntax)
                RunReturn(node as ReturnStatementSyntax);
            else if (node is YieldStatementSyntax)
                RunYield(node as YieldStatementSyntax);
            else if (node is BreakStatementSyntax)
                RunBreak(node as BreakStatementSyntax);
            else if (node is ContinueStatementSyntax)
                RunContinue(node as ContinueStatementSyntax);
            else if (node is LocalDeclarationStatementSyntax)
                RunLocalDeclaration(node as LocalDeclarationStatementSyntax);
            else if (node is LabeledStatementSyntax)
                RunLabeled(node as LabeledStatementSyntax);
            else if (node is VariableDeclarationSyntax)
                RunVariableDeclaration(node as VariableDeclarationSyntax);
            else if (node is ExpressionStatementSyntax)
                RunExpressionStatement(node as ExpressionStatementSyntax);

            else if (node is LockStatementSyntax)
                RunLock(node as LockStatementSyntax);
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
        

        internal HybInstance RunMethod(SSMethodInfo method, HybInstance[] args)
        {
            ret = null;
            ctx.PushMethod(method);

            var node = method.declaration;
            var vf = new VarFrame(null);
            var count = 0;

            foreach (var p in method.declaration.ParameterList.Parameters)
            {
                if (p.Default == null) continue;
                vf.SetValue(p.Identifier.Text, RunExpression(p.Default.Value));
            }
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
            {
                if (method.returnType.isCompiledType &&
                    method.returnType.compiledType == typeof(IEnumerator))
                {
                    var enumerator = new SSEnumerator(this, node.Body, vf);
                    ret = HybInstance.Object(enumerator);
                }
                else
                    RunBlock(node.Body, vf);
            }
            else
                ret = RunArrowExpressionClause(node.ExpressionBody, vf);

            vars = frames.Pop();

            if (halt == HaltType.Return)
                halt = HaltType.None;

            ctx.PopMethod();

            return ret;
        }
        internal HybInstance RunMethod(HybInstance _this, SSMethodInfo method, HybInstance[] args)
        {
            BindThis(_this);
            return RunMethod(method, args);
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
    }
}
