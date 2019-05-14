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

        /// <summary>
        /// Whether method has `params` modifier in last parameter.
        /// </summary>
        public bool isVaArg { get; }
        public HybType returnType { get; }

        public SSParamInfo[] parameters { get; }

        internal JumpDestination[] jumps;
        internal BaseMethodDeclarationSyntax declaration;

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

            this.parameters = new SSParamInfo[declaration.ParameterList.Parameters.Count];
            for (int i = 0; i < this.parameters.Length; i++)
            {
                var p = declaration.ParameterList.Parameters[i];
                this.parameters[i] = new SSParamInfo()
                {
                    id = p.Identifier.Text,
                    defaultValue = p.Default != null ? runner.RunExpression(p.Default.Value) : null,
                    isParams = p.Modifiers.IsParams()
                };
            }
        }
        internal SSMethodInfo(Runner runner, string id, HybType declaringType, Invokable body, HybType[] parameterTypes, HybType returnType)
        {
            this.id = id;
            this.signature = id; // ??
            this.declaringType = declaringType;
            this.target = body;

            this.returnType = returnType;
            this.isVaArg = false; // for now

            this.parameters = new SSParamInfo[parameterTypes.Length];
            for (int i = 0; i < this.parameters.Length; i++)
            {
                this.parameters[i] = new SSParamInfo()
                {
                    id = $"{i}",
                    defaultValue = null,
                    isParams = false
                };
            }
        }
        internal SSMethodInfo(MethodInfo methodInfo)
        {
            this.id = methodInfo.Name;
            this.signature = MemberSignature.GetSignature(methodInfo);
            this.declaringType = HybTypeCache.GetHybType(methodInfo.DeclaringType);
            this.target = new Invokable(methodInfo);

            this.returnType = HybTypeCache.GetHybType(methodInfo.ReturnType);
            this.isVaArg =
                methodInfo.GetParameters().LastOrDefault()
                ?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;

            var ps = methodInfo.GetParameters();
            this.parameters = new SSParamInfo[ps.Length];
            for (int i = 0; i < this.parameters.Length; i++)
            {
                var p = ps[i];
                this.parameters[i] = new SSParamInfo()
                {
                    id = p.Name,
                    defaultValue = p.HasDefaultValue ? HybInstance.Object(p.DefaultValue) : null,
                    isParams = this.isVaArg && i == this.parameters.Length - 1
                };
            }
        }

        public Type[] GetGenericArgumentsFromDefinition()
        {
            if (target.isCompiled)
            {
                if (target.compiledMethod.IsGenericMethod)
                {
                    return target.compiledMethod
                        .GetGenericMethodDefinition()
                        .GetGenericArguments();
                }
            }
            return new Type[] { };
        }
        public int GetGenericArgumentCount()
        {
            if (target.isCompiled)
                return target.compiledMethod.GetGenericArguments().Length;
            return 0;
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

        public HybInstance Invoke(HybInstance _this, params HybInstance[] args)
            => target.Invoke(_this, args);
    }
}
