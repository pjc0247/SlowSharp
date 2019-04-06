using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal static class RoslynExt
    {
        public static string GetPureName(this NameSyntax _this)
        {
            if (_this is GenericNameSyntax gn)
                return gn.Identifier.Text;
            return $"{_this}";
        }
    }
}
