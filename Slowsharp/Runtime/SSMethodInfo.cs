using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class SSMethodInfo : SSMemberInfo
    {
        public Invokable target;
        public BaseMethodDeclarationSyntax declaration;

        /// <summary>
        /// Whether method has `params` modifier in last parameter.
        /// </summary>
        public bool isVaArg { get; }
        public HybType returnType { get; }

        internal JumpDestination[] jumps;

        internal SSMethodInfo(Runner runner, BaseMethodDeclarationSyntax declaration)
        {
            target = new Invokable(this, runner, declaration);

            if (declaration is MethodDeclarationSyntax md)
                returnType = runner.resolver.GetType($"{md.ReturnType}");
            isVaArg =
                declaration.ParameterList.Parameters.LastOrDefault()
                ?.Modifiers.IsParams() ?? false;
        }
        internal SSMethodInfo(MethodInfo methodInfo)
        {
            target = new Invokable(this, methodInfo);

            returnType = new HybType(methodInfo.ReturnType);
            isVaArg =
                methodInfo.GetParameters().LastOrDefault()
                ?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;
        }

        public SSMethodInfo MakeGenericMethod(HybType[] genericArgs)
        {
            if (target.isCompiled)
            {
                var method = target.compiledMethod
                    .MakeGenericMethod(genericArgs.Unwrap());
                return new SSMethodInfo(method);
            }

            throw new NotImplementedException();
        }
    }
}
