using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
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

    internal static class AccessModifierEvaluator
    {
        public static bool IsAcceesible(this AccessModifier _this, AccessLevel level)
        {
            if (_this == AccessModifier.Protected)
            {
                if (level == AccessLevel.Outside)
                    return false;
            }
            else if (_this == AccessModifier.Private)
            {
                if (level == AccessLevel.Outside ||
                    level == AccessLevel.Derivered)
                    return false;
            }
            return true;
        }
    }
    internal class AccessModifierParser
    {
        public static AccessModifier Get(MethodBase method)
        {
            if (method.IsPublic) return AccessModifier.Public;
            if (method.IsPrivate) return AccessModifier.Private;
            return AccessModifier.Protected;
        }
        public static AccessModifier Get(FieldInfo field)
        {
            if (field.IsPublic) return AccessModifier.Public;
            if (field.IsPrivate) return AccessModifier.Private;
            return AccessModifier.Protected;
        }

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
