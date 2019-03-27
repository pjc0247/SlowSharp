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

        public object Invoke(object _this, object[] args)
        {
            if (isCompiled)
                Console.WriteLine($"Invoke {compiledMethod.Name}");
            else
                Console.WriteLine($"Invoke ");

            if (isCompiled)
            {
                return compiledMethod.Invoke(_this, args);
            }
            else
            {
                return runner.RunMethod(_this as HybInstance, interpretMethod);
            }
        }
    }
}
