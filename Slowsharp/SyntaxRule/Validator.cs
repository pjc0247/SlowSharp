using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal partial class Validator : CSharpSyntaxWalker
    {
        public override void VisitIncompleteMember(IncompleteMemberSyntax node)
        {
            base.VisitIncompleteMember(node);

            throw new SemanticViolationException($"Unrecognized syntax: {node}");
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);

            node.Left
                .ShouldBe<MemberAccessExpressionSyntax>()
                .ShouldBe<ElementAccessExpressionSyntax>()
                .ShouldBe<ImplicitElementAccessSyntax>()
                .ShouldBe<IdentifierNameSyntax>()
                .ThrowIfNot();
        }
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            node.Type
                .ShouldBe<TypeSyntax>()
                .ShouldBe<IdentifierNameSyntax>()
                .ShouldNotEmptyIdent()
                .ThrowIfNot();
        }
    }
}
