using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class Invokable
    {
        private enum InvokeType
        {
            Interpret,
            ReflectionInvoke,
            FuncInvoke
        }

        public bool IsCompiled => CompiledMethod != null;

        internal BaseMethodDeclarationSyntax InterpretMethod { get; }
        internal MethodInfo CompiledMethod { get; }
        internal Func<HybInstance[], HybInstance> FuncMethod { get; }
        private InvokeType Type { get; }

        private Runner Runner;
        private SSInterpretMethodInfo MethodInfo;

        public Invokable(SSInterpretMethodInfo methodInfo, Runner runner, BaseMethodDeclarationSyntax declaration)
        { 
            this.Type = InvokeType.Interpret;
            this.MethodInfo = methodInfo;
            this.Runner = runner;
            this.InterpretMethod = declaration;
        }
        public Invokable(MethodInfo method)
        {
            this.Type = InvokeType.ReflectionInvoke;
            this.CompiledMethod = method;
        }
        public Invokable(Runner runner, Func<HybInstance[], HybInstance> func)
        {
            this.Runner = runner;
            this.Type = InvokeType.FuncInvoke;
            this.FuncMethod = func;
        }

        public HybInstance Invoke(HybInstance _this, HybInstance[] args, bool hasRefOrOut = false)
        {
            if (IsCompiled)
                C.WriteLine($"Invoke {CompiledMethod.Name}");
            else
                C.WriteLine($"Invoke ");

            if (Type == InvokeType.ReflectionInvoke)
            {
                if (CompiledMethod.GetParameters().Length > args.Length)
                    args = ExpandArgs(args, CompiledMethod);

                var unwrappedArgs = args.Unwrap();
                var ret = CompiledMethod.Invoke(_this == null ? null : _this.IsCompiledType ? _this.InnerObject:_this.Parent.InnerObject, unwrappedArgs);

                if (hasRefOrOut)
                {
                    for (int i = 0; i < args.Length; i++)
                        args[i] = HybInstance.Object(unwrappedArgs[i]);
                }

                return HybInstance.Object(ret);
            }
            else if (Type == InvokeType.FuncInvoke)
            {
                return Runner.RunWrappedFunc(_this, FuncMethod, args);
            }
            else if (Type == InvokeType.Interpret)
            {
                var ps = InterpretMethod.ParameterList.Parameters;
                //if (args.Length != ps.Count)
                    //throw new SemanticViolationException($"Parameters.Count does not match");

                return Runner.RunMethod(
                    _this as HybInstance, MethodInfo, args);
            }

            throw new NotImplementedException($"Unknown type: {Type}");
        }

        private HybInstance[] ExpandArgs(HybInstance[] args, MethodInfo info)
        {
            var ps = info.GetParameters();
            var expanded = new HybInstance[ps.Length];

            for (int i = 0; i < ps.Length; i++)
            {
                if (args.Length > i)
                    expanded[i] = args[i];
                else
                    expanded[i] = HybInstance.Object(ps[i].DefaultValue);
            }

            return expanded;
        }
    }
}
