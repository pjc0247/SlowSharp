using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private void AddUsing(UsingDirectiveSyntax node)
        {
            lookup.Add($"{node.Name}");
        }
        private void AddClass(ClassDeclarationSyntax node)
        {
            klass = new Class(this, $"{node.Identifier}");
            ctx.types.Add($"{node.Identifier}", klass);
        }
        private void AddField(FieldDeclarationSyntax node)
        {
            foreach (var f in node.Declaration.Variables)
            {
                klass.AddField($"{f.Identifier}", node, f);
            }
        }
        private void AddConstructorMethod(ConstructorDeclarationSyntax node)
        {
            klass.AddMethod("$_ctor", node);
        }
        private void AddMethod(MethodDeclarationSyntax node)
        {
            klass.AddMethod(node.Identifier.ValueText, node);
        }
    }
}
