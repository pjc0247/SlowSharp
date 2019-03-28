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
    internal class Invokable
    {
        public bool isCompiled => compiledMethod != null;
        public BaseMethodDeclarationSyntax interpretMethod { get; }
        public MethodInfo compiledMethod { get; }

        private Runner runner;

        public Invokable(Runner runner, BaseMethodDeclarationSyntax method)
        {
            this.runner = runner;
            this.interpretMethod = method;
        }
        public Invokable(MethodInfo method)
        {
            this.compiledMethod = method;
        }

        public HybInstance Invoke(HybInstance _this, HybInstance[] args)
        {
            if (isCompiled)
                Console.WriteLine($"Invoke {compiledMethod.Name}");
            else
                Console.WriteLine($"Invoke ");

            if (isCompiled)
            {
                return HybInstance.Object(compiledMethod.Invoke(
                    _this?.innerObject, args.Unwrap()));
            }
            else
            {
                var ps = interpretMethod.ParameterList.Parameters;
                if (args.Length != ps.Count)
                    throw new SemanticViolationException($"Parameters.Count does not match");

                return runner.RunMethod(_this as HybInstance, interpretMethod, args);
            }
        }
    }
}
