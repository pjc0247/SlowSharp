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

        internal SSMethodInfo(Runner runner, string id, HybType declaringType, BaseMethodDeclarationSyntax declaration)
        {
            this.id = id;
            this.signature = MemberSignature.GetSignature(
                runner.resolver, id, declaration);
            this.declaringType = declaringType;
            this.target = new Invokable(this, runner, declaration);

            if (declaration is MethodDeclarationSyntax md)
                this.returnType = runner.resolver.GetType($"{md.ReturnType}");
            this.isVaArg =
                declaration.ParameterList.Parameters.LastOrDefault()
                ?.Modifiers.IsParams() ?? false;
        }
        internal SSMethodInfo(MethodInfo methodInfo)
        {
            this.id = methodInfo.Name;
            this.signature = MemberSignature.GetSignature(methodInfo);
            this.declaringType = new HybType(methodInfo.DeclaringType);
            this.target = new Invokable(this, methodInfo);

            this.returnType = new HybType(methodInfo.ReturnType);
            this.isVaArg =
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
