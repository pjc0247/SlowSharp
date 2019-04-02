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
        public Invokable(SSMethodInfo methodInfo, MethodInfo method)
        {
            this.type = InvokeType.ReflectionInvoke;
            this.methodInfo = methodInfo;
            this.compiledMethod = method;
        }
        public Invokable(Func<HybInstance[], HybInstance> func)
        {
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
    }
}
