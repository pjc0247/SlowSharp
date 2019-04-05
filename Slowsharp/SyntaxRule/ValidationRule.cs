using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal static class ValudationRuleExt
    {
        // Method name `Accept` is already taken by Roslyn.
        public static ValidationRule ShouldBe<TAccept>(this CSharpSyntaxNode _this)
        {
            return new ValidationRule(_this)
                .ShouldBe<TAccept>();
        }
    }
    internal class ValidationRule
    {
        private List<Type> accept = new List<Type>();
        private List<Type> deny = new List<Type>();
        private object obj;

        public ValidationRule(object obj)
        {
            this.obj = obj;
        }

        public ValidationRule ShouldNotEmptyIdent()
        {
            if (obj is IdentifierNameSyntax id &&
                string.IsNullOrEmpty(id.Identifier.Text))
            {
                throw new SemanticViolationException($"ident should not be empty");
            }
            return this;
        }
        public ValidationRule ShouldBe<T>()
        {
            accept.Add(typeof(T));
            return this;
        }
        public ValidationRule ShouldNotBe<T>()
        {
            deny.Add(typeof(T));
            return this;
        }
        public ValidationRule ShouldNotBeIdent(string ident)
        {
            if (obj is IdentifierNameSyntax id &&
                id.Identifier.Text == ident)
            {
                throw new SemanticViolationException($"illigal ident: {ident}");
            }
            return this;
        }
        public void ThrowIfNot()
        {
            if (obj == null)
                return;

            var pass = false;
            foreach (var type in accept)
            {
                //if (obj.GetType().IsAssignableFrom(acc))
                if (type.IsAssignableFrom(obj.GetType()))
                {
                    pass = true;
                    break;
                }
            }
            foreach (var type in deny)
            {
                if (type.IsAssignableFrom(obj.GetType()))
                {
                    pass = false;
                    break;
                }
            }

            if (pass == false)
            {
#if DEBUG
                throw new SemanticViolationException($"{obj}, {obj.GetType()} is not expected");
#else
                throw new SemanticViolationException($"{obj} is not expected");
#endif
            }
        }
    }
}
