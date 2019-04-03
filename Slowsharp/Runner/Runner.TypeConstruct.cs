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
            var id = $"{node.Identifier}";

            if (ctx.types.ContainsKey(id))
                throw new SemanticViolationException($"Class redefination is not supported: {id}");

            HybType parentType = null;
            if (node.BaseList != null)
            {
                foreach (var b in node.BaseList.Types)
                {
                    var type = resolver.GetType($"{b.Type}");
                    if (type.isInterface == false)
                        parentType = type;
                }
            }

            klass = new Class(this, id, parentType);
            ctx.types.Add(id, klass);
        }
        private void AddProperty(PropertyDeclarationSyntax node)
        {
            var isStatic = node.Modifiers.IsStatic();
            var type = resolver.GetType($"{node.Type}");
            var id = $"{node.Identifier}";

            var propertyInfo = klass.AddProperty(id, node);

            if (propertyInfo.isStatic)
                InitializeStaticProperty(propertyInfo);
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
                    InitializeStaticField(f, id, type);
            }
        }

        private void InitializeStaticProperty(SSPropertyInfo info)
        {
            if (info.hasBackingField == false)
                return;

            var backingField = info.backingField;

            if (info.property.Initializer == null)
                globals.SetStaticField(klass, backingField.id, info.type.GetDefault());
            else
            {
                var capturedKlass = klass;
                AddLazyInitializer(() =>
                {
                    globals.SetStaticField(
                        capturedKlass,
                        backingField.id, RunExpression(info.property.Initializer.Value));
                });
            }
        }
        private void InitializeStaticField(VariableDeclaratorSyntax field, string id, HybType type)
        {
            if (field.Initializer == null)
                globals.SetStaticField(klass, id, type.GetDefault());
            else
            {
                var capturedKlass = klass;
                AddLazyInitializer(() =>
                {
                    globals.SetStaticField(
                        capturedKlass,
                        id, RunExpression(field.Initializer.Value));
                });
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
