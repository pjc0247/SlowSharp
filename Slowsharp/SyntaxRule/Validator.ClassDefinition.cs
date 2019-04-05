using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal partial class Validator
    {
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            base.VisitFieldDeclaration(node);

            node.Declaration.Type
                .ShouldBe<TypeSyntax>()
                .ShouldNotBeIdent("var")
                .ThrowIfNot();
        }
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            base.VisitPropertyDeclaration(node);

            node.Type
                .ShouldBe<TypeSyntax>()
                .ShouldNotBeIdent("var")
                .ThrowIfNot();
        }
    }
}
