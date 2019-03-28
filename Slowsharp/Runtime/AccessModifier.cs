using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace Slowsharp
{
    [Flags]
    internal enum AccessModifier
    {
        None = 0,

        Public = 1, 
        Protected = 2,
        Private = 4,
        Internal = 8
    }

    internal class AccessModifierParser
    {
        public static AccessModifier Parse(SyntaxTokenList list)
        {
            AccessModifier mod = AccessModifier.None;
            foreach (var item in list)
            {
                var raw = item.Text;

                if (raw == "public")
                    mod |= AccessModifier.Public;
                else if (raw == "protected")
                    mod |= AccessModifier.Protected;
                else if (raw == "private")
                    mod |= AccessModifier.Private;
                else if (raw == "internal")
                    mod |= AccessModifier.Internal;
            }

            if (mod.HasFlag(AccessModifier.Public))
            {
                if (mod.HasFlag(AccessModifier.Private) ||
                    mod.HasFlag(AccessModifier.Protected))
                    throw new SemanticViolationException("Invalid access modifier");
            }
            else if (mod.HasFlag(AccessModifier.Protected))
            {
                if (mod.HasFlag(AccessModifier.Private))
                    throw new SemanticViolationException("Invalid access modifier");
            }

            if (mod == AccessModifier.None)
                mod = AccessModifier.Private;

            return mod;
        }
    }
}
