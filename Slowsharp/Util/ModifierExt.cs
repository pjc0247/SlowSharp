using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Slowsharp
{
    internal static class ModifierExt
    {
        public static bool IsStatic(this SyntaxTokenList _this)
        {
            foreach (var token in _this)
            {
                if (token.Text == "static")
                    return true;
            }
            return false;
        }
    }
}
