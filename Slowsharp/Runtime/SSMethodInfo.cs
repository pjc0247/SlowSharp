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
        public Invokable Target;

        /// <summary>
        /// Whether method has `params` modifier in last parameter.
        /// </summary>
        public bool IsVaArg { get; }
        public HybType ReturnType { get; }

        public SSParamInfo[] Parameters { get; }

        internal JumpDestination[] Jumps;
        internal BaseMethodDeclarationSyntax Declaration;

        internal SSMethodInfo(Runner runner, string id, HybType declaringType, BaseMethodDeclarationSyntax declaration)
        {
            this.Id = id;
            this.Signature = MemberSignature.GetSignature(
                runner.Resolver, id, declaration);
            this.DeclaringType = declaringType;
            this.Target = new Invokable(this, runner, declaration);

            if (declaration is MethodDeclarationSyntax md)
                this.ReturnType = runner.Resolver.GetType($"{md.ReturnType}");
            this.IsVaArg =
                declaration.ParameterList.Parameters.LastOrDefault()
                ?.Modifiers.IsParams() ?? false;

            this.Parameters = new SSParamInfo[declaration.ParameterList.Parameters.Count];
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                var p = declaration.ParameterList.Parameters[i];
                this.Parameters[i] = new SSParamInfo()
                {
                    Id = p.Identifier.Text,
                    DefaultValue = p.Default != null ? runner.RunExpression(p.Default.Value) : null,
                    IsParams = p.Modifiers.IsParams()
                };
            }
        }
        internal SSMethodInfo(Runner runner, string id, HybType declaringType, Invokable body, HybType[] parameterTypes, HybType returnType)
        {
            this.Id = id;
            this.Signature = id; // ??
            this.DeclaringType = declaringType;
            this.Target = body;

            this.ReturnType = returnType;
            this.IsVaArg = false; // for now

            this.Parameters = new SSParamInfo[parameterTypes.Length];
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                this.Parameters[i] = new SSParamInfo()
                {
                    Id = $"{i}",
                    DefaultValue = null,
                    IsParams = false
                };
            }
        }
        internal SSMethodInfo(MethodInfo methodInfo)
        {
            this.Id = methodInfo.Name;
            this.Signature = MemberSignature.GetSignature(methodInfo);
            this.DeclaringType = HybTypeCache.GetHybType(methodInfo.DeclaringType);
            this.Target = new Invokable(methodInfo);

            this.ReturnType = HybTypeCache.GetHybType(methodInfo.ReturnType);
            this.IsVaArg =
                methodInfo.GetParameters().LastOrDefault()
                ?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;

            var ps = methodInfo.GetParameters();
            this.Parameters = new SSParamInfo[ps.Length];
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                var p = ps[i];
                this.Parameters[i] = new SSParamInfo()
                {
                    Id = p.Name,
                    DefaultValue = p.HasDefaultValue ? HybInstance.Object(p.DefaultValue) : null,
                    IsParams = this.IsVaArg && i == this.Parameters.Length - 1
                };
            }
        }

        public Type[] GetGenericArgumentsFromDefinition()
        {
            if (Target.IsCompiled)
            {
                if (Target.CompiledMethod.IsGenericMethod)
                {
                    return Target.CompiledMethod
                        .GetGenericMethodDefinition()
                        .GetGenericArguments();
                }
            }
            return new Type[] { };
        }
        public int GetGenericArgumentCount()
        {
            if (Target.IsCompiled)
                return Target.CompiledMethod.GetGenericArguments().Length;
            return 0;
        }
        public SSMethodInfo MakeGenericMethod(HybType[] genericArgs)
        {
            if (Target.IsCompiled)
            {
                var method = Target.CompiledMethod
                    .MakeGenericMethod(genericArgs.Unwrap());
                return new SSMethodInfo(method);
            }

            throw new NotImplementedException();
        }

        public HybInstance Invoke(HybInstance _this, params HybInstance[] args)
            => Target.Invoke(_this, args);
    }
}
