using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class Validator : CSharpSyntaxWalker
    {
        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);

            node.Left
                .RuleAccept<MemberAccessExpressionSyntax>()
                .RuleAccept<IdentifierNameSyntax>()
                .ThrowIfNot();
        }
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            node.Type
                .RuleAccept<IdentifierNameSyntax>()
                .ShouldNotEmptyIdent()
                .ThrowIfNot();
        }
    }
}
