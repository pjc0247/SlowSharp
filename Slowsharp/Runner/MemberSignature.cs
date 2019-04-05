using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class MemberSignature
    {
        public static string GetSignature(MethodInfo methodInfo)
        {
            var sb = new StringBuilder($"{methodInfo.Name}_");

            foreach (var p in methodInfo.GetParameters())
                sb.Append($"{p.ParameterType.FullName},");

            return sb.ToString();
        }
        public static string GetSignature(TypeResolver resolver, string id, BaseMethodDeclarationSyntax method)
        {
            var sb = new StringBuilder($"{id}_");
            var ps = method.ParameterList.Parameters;
            foreach(var p in ps)
            {
                var type = resolver.GetType($"{p.Type}");
                sb.Append($"{type.fullName},");
            }
            return sb.ToString();
        }
    }
}
