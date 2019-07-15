using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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

        public static void ShouldBeNotEmpty(this SyntaxToken _this, string expected)
        {
            if (_this.Text == "")
                throw new SemanticViolationException($"Missing token. (Expected: {expected})");
        }
    }
    internal class ValidationRule
    {
        private List<Type> Accept = new List<Type>();
        private List<Type> Deny = new List<Type>();
        private object Obj;

        public ValidationRule(object obj)
        {
            this.Obj = obj;
        }

        public ValidationRule ShouldNotEmptyIdent()
        {
            if (Obj is IdentifierNameSyntax id &&
                string.IsNullOrEmpty(id.Identifier.Text))
            {
                throw new SemanticViolationException($"ident should not be empty");
            }
            return this;
        }
        public ValidationRule ShouldBe<T>()
        {
            Accept.Add(typeof(T));
            return this;
        }
        public ValidationRule ShouldNotBe<T>()
        {
            Deny.Add(typeof(T));
            return this;
        }
        public ValidationRule ShouldNotBeIdent(string ident)
        {
            if (Obj is IdentifierNameSyntax id &&
                id.Identifier.Text == ident)
            {
                throw new SemanticViolationException($"illigal ident: {ident}");
            }
            return this;
        }
        public void ThrowIfNot()
        {
            if (Obj == null)
                return;

            var pass = false;
            foreach (var type in Accept)
            {
                //if (obj.GetType().IsAssignableFrom(acc))
                if (type.IsAssignableFrom(Obj.GetType()))
                {
                    pass = true;
                    break;
                }
            }
            foreach (var type in Deny)
            {
                if (type.IsAssignableFrom(Obj.GetType()))
                {
                    pass = false;
                    break;
                }
            }

            if (pass == false)
            {
#if DEBUG
                throw new SemanticViolationException($"{Obj}, {Obj.GetType()} is not expected");
#else
                throw new SemanticViolationException($"{Obj} is not expected");
#endif
            }
        }
    }
}
