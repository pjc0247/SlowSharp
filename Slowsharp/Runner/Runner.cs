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
        internal ScriptConfig ScriptConfig { get; }
        internal RunContext Ctx;
        internal GlobalStorage Globals { get; }
        internal ExtensionMethodResolver ExtResolver { get; }
        internal TypeResolver Resolver { get; private set; }
        private IdLookup Lookup;
        private Class Klass;
        internal VarFrame Vars { get; private set; }
        private Stack<CatchFrame> Catches;
        private OptNodeCache OptCache;

        private Stack<VarFrame> Frames { get; }

        internal HybInstance Ret;
        private HaltType Halt;

        private RunMode RunMode;

        private Dictionary<string, SyntaxNode> PendingSyntaxs = new Dictionary<string, SyntaxNode>();
        private HashSet<SyntaxNode> LoadedSyntaxs = new HashSet<SyntaxNode>();

        public Runner(ScriptConfig scriptConfig, RunConfig config)
        {
            this.Ctx = new RunContext(config);
            this.ScriptConfig = scriptConfig;
            this.Globals = new GlobalStorage();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            this.OptCache = new OptNodeCache();
            this.Lookup = new IdLookup(assemblies);
            this.Catches = new Stack<CatchFrame>();
            this.Frames = new Stack<VarFrame>();
            this.ExtResolver = new ExtensionMethodResolver(assemblies);
            this.Resolver = new TypeResolver(Ctx, assemblies);

            AddDefaultUsings();
            PrewarmTypes();
        }
        private void AddDefaultUsings()
        {
            foreach (var ns in ScriptConfig.DefaultUsings)
                Resolver.AddLookupNamespace(ns);
        }
        private void PrewarmTypes()
        {
            foreach (var type in ScriptConfig.PrewarmTypes)
                Resolver.CacheType(type);
        }

        public HybType[] GetTypes()
        {
            return Ctx.Types
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
            RunMode = RunMode.Parse;
            Run(node);
            RunMode = RunMode.Execution;
        }
        public void LoadSyntax(SyntaxNode[] nodes)
        {
            RunMode = RunMode.Preparse;
            foreach (var node in nodes)
                Run(node);
            RunMode = RunMode.Parse;
            foreach (var node in nodes)
                Run(node);
            RunMode = RunMode.Execution;
        }
        public void UpdateMethodsOnly(SyntaxNode node)
        {
            RunMode = RunMode.HotLoadMethodsOnly;
            Run(node);
            RunMode = RunMode.Execution;
        }

        internal void BindThis(HybInstance _this)
        {
            Ctx._this = _this;
        }

        public HybInstance RunScriptStatement(StatementSyntax node)
        {
            Ctx.PushMethod(new SSInterpretMethodInfo(this, 
                "__scripteval", 
                HybTypeCache.Void, null, new HybType[] { }, HybTypeCache.Void));
            RunMode = RunMode.Execution;

            Run(node);
            Ctx.PopMethod();
            return Ret;
        }

        internal HybInstance RunMain(params object[] args)
        {
            Ctx.Reset();
            RunLazyInitializers();

            foreach (var type in Ctx.Types)
            {
                var mains = type.Value.GetMethods("Main");
                if (mains.Length == 0)
                    continue;

                return mains[0].Target.Invoke(null, args.Wrap());
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
            return Resolver.GetType(id).CreateInstance(this, args.Wrap());
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
            return Resolver.GetType(id).Override(this, args.Wrap(), parentObject);
        }

        public void Run(SyntaxNode node)
        {
            Ctx.LastNode = node;

            var treatAsBlock = new Type[]
            {
                typeof(CompilationUnitSyntax),
                typeof(NamespaceDeclarationSyntax),
                typeof(ClassDeclarationSyntax),
                typeof(EnumDeclarationSyntax)
            };

            switch (RunMode)
            {
                case RunMode.Preparse:
                    RunAsPreparse(node);
                    break;
                case RunMode.Parse:
                    RunAsParse(node);
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

            if (Ctx.IsExpird())
                throw new TimeoutException();
        }
        public void RunAsPreparse(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax cd)
                PendingSyntaxs[cd.Identifier.Text] = node;
        }
        public void RunAsParse(SyntaxNode node)
        {
            if (LoadedSyntaxs.Contains(node))
                return;

            if (node is UsingDirectiveSyntax)
                AddUsing(node as UsingDirectiveSyntax);
            else if (node is ClassDeclarationSyntax)
                AddClass(node as ClassDeclarationSyntax);
            else if (node is ConstructorDeclarationSyntax)
                AddConstructorMethod(node as ConstructorDeclarationSyntax);
            else if (node is IndexerDeclarationSyntax)
                AddIndexer(node as IndexerDeclarationSyntax);
            else if (node is PropertyDeclarationSyntax)
                AddProperty(node as PropertyDeclarationSyntax);
            else if (node is FieldDeclarationSyntax)
                AddField(node as FieldDeclarationSyntax);
            else if (node is MethodDeclarationSyntax)
                AddMethod(node as MethodDeclarationSyntax);
            else if (node is EnumDeclarationSyntax)
                AddEnum(node as EnumDeclarationSyntax);
            else if (node is EnumMemberDeclarationSyntax)
                AddEnumMember(node as EnumMemberDeclarationSyntax);

            LoadedSyntaxs.Add(node);
        }
        public void RunAsHotReloadMethodsOnly(SyntaxNode node)
        {
            if (node is UsingDirectiveSyntax)
                AddUsing(node as UsingDirectiveSyntax);
            else if (node is ClassDeclarationSyntax classDeclaration)
                Klass = Ctx.Types[classDeclaration.Identifier.Text];
            else if (node is MethodDeclarationSyntax)
                AddMethod(node as MethodDeclarationSyntax);
        }
        public void RunAsExecution(SyntaxNode node)
        {
            Halt = HaltType.None;
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
            Vars = vf;
            var ret = RunExpression(node.Expression);
            Vars = Vars.Parent;
            return ret;
        }

        internal HybInstance RunMethod(SSInterpretMethodInfo method, HybInstance[] args)
        {
            Ret = null;
            Ctx.PushMethod(method);

            var node = ((SSInterpretMethodInfo)method).Declaration;
            var vf = new VarFrame(null);
            var count = 0;

            foreach (var p in method.Parameters)
            {
                if (p.DefaultValue == null) continue;
                vf.SetValue(p.Id, p.DefaultValue);
            }
            foreach (var arg in args)
            {
                var p = method.Parameters[count++];
                if (p.IsParams)
                    break;

                vf.SetValue(p.Id, arg);
            }

            if (method.IsVaArg)
            {
                var paramId = node.ParameterList.Parameters.Last()
                    .Identifier.Text;

                var vaArgs = args.Skip(count - 1).ToArray();
                vf.SetValue(paramId, HybInstance.ObjectArray(vaArgs));
            }

            Frames.Push(Vars);
            Vars = null;

            if (node.Body != null)
            {
                if (method.ReturnType != null && // ctor doesn't have return type
                    method.ReturnType.IsCompiledType &&
                    method.ReturnType.CompiledType == typeof(IEnumerator))
                {
                    var enumerator = new SSEnumerator(this, node.Body, vf);
                    enumerator.Method = method;
                    Ret = HybInstance.Object(enumerator);
                }
                else
                    RunBlock(node.Body, vf);
            }
            else
                Ret = RunArrowExpressionClause(node.ExpressionBody, vf);

            Vars = Frames.Pop();

            if (Halt == HaltType.Return)
                Halt = HaltType.None;

            Ctx.PopMethod();

            return Ret;
        }
        /// <summary>
        /// Run method with `_this`.
        /// </summary>
        internal HybInstance RunMethod(HybInstance _this, SSInterpretMethodInfo method, HybInstance[] args)
        {
            BindThis(_this);
            return RunMethod(method, args);
        }
        internal HybInstance RunWrappedFunc(HybInstance _this, Func<HybInstance[], HybInstance> func, HybInstance[] args)
        {
            BindThis(_this);
            var ret = func.Invoke(args);
            if (Halt == HaltType.Return)
                Halt = HaltType.None;
            return ret;
        }

        /// <summary>
        /// Update variable with given key and value.
        /// </summary>
        private void UpdateVariable(string key, HybInstance value)
        {
            // 1. Local variable
            if (Vars.UpdateValue(key, value) == false)
            {
                if (Ctx._this != null)
                {
                    // 2. Member property
                    if (Ctx._this.SetPropertyOrField(key, value, AccessLevel.This))
                        return;
                }
                // 3. Static property
                if (Ctx.Method.DeclaringType.SetStaticPropertyOrField(key, value, AccessLevel.This))
                    return;
            }
            else
                return;

            throw new SemanticViolationException($"No such variable in current context: {key}.");
        }
    }
}
