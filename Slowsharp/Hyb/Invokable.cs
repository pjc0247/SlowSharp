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

        public bool isCompiled => compiledMethod != null;

        internal BaseMethodDeclarationSyntax interpretMethod { get; }
        internal MethodInfo compiledMethod { get; }
        internal Func<HybInstance[], HybInstance> funcMethod { get; }
        private InvokeType type { get; }

        private Runner runner;
        private SSMethodInfo methodInfo;

        public Invokable(SSMethodInfo methodInfo, Runner runner, BaseMethodDeclarationSyntax declaration)
        { 
            this.type = InvokeType.Interpret;
            this.methodInfo = methodInfo;
            this.runner = runner;
            this.interpretMethod = declaration;
        }
        public Invokable(MethodInfo method)
        {
            this.type = InvokeType.ReflectionInvoke;
            this.compiledMethod = method;
        }
        public Invokable(Runner runner, Func<HybInstance[], HybInstance> func)
        {
            this.runner = runner;
            this.type = InvokeType.FuncInvoke;
            this.funcMethod = func;
        }

        public HybInstance Invoke(HybInstance _this, HybInstance[] args, bool hasRefOrOut = false)
        {
            if (isCompiled)
                Console.WriteLine($"Invoke {compiledMethod.Name}");
            else
                Console.WriteLine($"Invoke ");

            if (type == InvokeType.ReflectionInvoke)
            {
                if (compiledMethod.GetParameters().Length > args.Length)
                    args = ExpandArgs(args, compiledMethod);

                var unwrappedArgs = args.Unwrap();
                var ret = compiledMethod.Invoke(_this?.innerObject, unwrappedArgs);

                if (hasRefOrOut)
                {
                    for (int i = 0; i < args.Length; i++)
                        args[i] = HybInstance.Object(unwrappedArgs[i]);
                }

                return HybInstance.Object(ret);
            }
            else if (type == InvokeType.FuncInvoke)
            {
                runner.BindThis(_this);
                return funcMethod.Invoke(args);
            }
            else if (type == InvokeType.Interpret)
            {
                var ps = interpretMethod.ParameterList.Parameters;
                //if (args.Length != ps.Count)
                    //throw new SemanticViolationException($"Parameters.Count does not match");

                return runner.RunMethod(
                    _this as HybInstance, methodInfo, args);
            }

            throw new NotImplementedException($"Unknown type: {type}");
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
