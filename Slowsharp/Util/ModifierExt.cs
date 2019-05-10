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
        public static bool Contains(this SyntaxTokenList _this, string word)
        {
            foreach (var token in _this)
            {
                if (token.Text == word)
                    return true;
            }
            return false;
        }

        public static bool IsParams(this SyntaxTokenList _this)
            => Contains(_this, "params");
        public static bool IsStatic(this SyntaxTokenList _this)
            => Contains(_this, "static");
        public static bool IsConst(this SyntaxTokenList _this)
            => Contains(_this, "const");
    }
}
