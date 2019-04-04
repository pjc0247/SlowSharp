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
        public static ValidationRule RuleAccept<TAccept>(this CSharpSyntaxNode _this)
        {
            return new ValidationRule(_this)
                .RuleAccept<TAccept>();
        }
    }
    internal class ValidationRule
    {
        private List<Type> accept = new List<Type>();
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
        public ValidationRule RuleAccept<T>()
        {
            accept.Add(typeof(T));
            return this;
        }
        public void ThrowIfNot()
        {
            if (obj == null)
                return;

            var pass = false;
            foreach (var acc in accept)
            {
                if (obj.GetType().IsAssignableFrom(acc))
                {
                    pass = true;
                    break;
                }
            }
            if (pass == false)
                throw new SemanticViolationException($"{obj} is not expected");
        }
    }
}
