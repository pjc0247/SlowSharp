using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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

            if (node.BaseList == null)
                return;
            foreach (var b in node.BaseList.Types)
            {
                var type = resolver.GetType($"{b.Type}");
            }
        }
        private void AddProperty(PropertyDeclarationSyntax node)
        {
            klass.AddProperty($"{node.Identifier}", node);
        }
        private void AddField(FieldDeclarationSyntax node)
        {
            var isStatic = node.Modifiers.IsStatic();
            var type = resolver.GetType($"{node.Declaration.Type}");

            foreach (var f in node.Declaration.Variables)
            {
                var id = $"{f.Identifier}";

                klass.AddField(id, node, f);
                if (isStatic)
                {
                    globals.SetStaticField(klass, id, type.GetDefault());
                }
            }
        }
        private void AddConstructorMethod(ConstructorDeclarationSyntax node)
        {
            klass.AddMethod(
                "$_ctor", node,
                BuildJumps(node.Body));
        }
        private void AddMethod(MethodDeclarationSyntax node)
        {
            klass.AddMethod(
                node.Identifier.ValueText,
                node, 
                BuildJumps(node.Body));
        }

        private JumpDestination[] BuildJumps(BlockSyntax node)
        {
            // Method has ExpressionBody
            if (node == null)
                return new JumpDestination[] { };

            var jumps = new List<JumpDestination>();
            FindJumpsDownwards(node, jumps, 0);
            return jumps.ToArray();
        }
        private void FindJumpsDownwards(SyntaxNode node, List<JumpDestination> jumps, int depth)
        {
            var children = node.ChildNodes().ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];

                if (child is LabeledStatementSyntax lb)
                {
                    jumps.Add(new JumpDestination() {
                        label = lb.Identifier.Text,
                        statement = (StatementSyntax)node,
                        pc = i,
                        frameDepth = depth
                    });
                }

                FindJumpsDownwards(child, jumps, depth + 1);
            }
        }
    }
}
