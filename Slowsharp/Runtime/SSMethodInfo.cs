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
        public bool IsVaArg { get; protected set; }
        public HybType ReturnType { get; protected set; }

        public SSParamInfo[] Parameters { get; protected set; }

        internal JumpDestination[] Jumps;

        protected SSMethodInfo()
        {
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
                return new SSCompiledMethodInfo(method);
            }

            throw new NotImplementedException();
        }

        public HybInstance Invoke(HybInstance _this, params HybInstance[] args)
            => Target.Invoke(_this, args);
    }
    public class SSCompiledMethodInfo : SSMethodInfo
    {
        internal SSCompiledMethodInfo(MethodInfo methodInfo)
        {
            this.Id = methodInfo.Name;
            this.Signature = MemberSignature.GetSignature(methodInfo);
            this.DeclaringType = HybTypeCache.GetHybType(methodInfo.DeclaringType);
            this.Target = new Invokable(methodInfo);
            this.IsStatic = methodInfo.IsStatic;
            this.AccessModifier = AccessModifierParser.Get(methodInfo);

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
    }
    public class SSInterpretMethodInfo : SSMethodInfo
    {
        internal BaseMethodDeclarationSyntax Declaration;

        private SSAttributeInfo[] Attributes;

        internal SSInterpretMethodInfo(Runner runner, string id, HybType declaringType, BaseMethodDeclarationSyntax declaration)
        {
            this.Id = id;
            this.Signature = MemberSignature.GetSignature(
                runner.Resolver, id, declaration);
            this.Declaration = declaration;
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
        internal SSInterpretMethodInfo(Runner runner, string id, HybType declaringType, Invokable body, HybType[] parameterTypes, HybType returnType)
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

            this.Attributes = new SSAttributeInfo[] { };
        }

        public override SSAttributeInfo[] GetCustomAttributes()
        {
            if (Attributes == null)
                BuildAttributes();
            return Attributes;
        }
        private void BuildAttributes()
        {
            if (Declaration == null)
            {
                Attributes = new SSAttributeInfo[] { };
                return;
            }

            var attributes = new List<SSAttributeInfo>();
            foreach (var attr in Declaration.AttributeLists
                .SelectMany(x => x.Attributes))
            {
                attributes.Add(new SSAttributeInfo(attr));
            }
            Attributes = attributes.ToArray();
        }
    }
}
