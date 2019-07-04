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
        private Dictionary<Class, SSInterpretMethodInfo> staticInitializers = new Dictionary<Class, SSInterpretMethodInfo>();

        private void AddUsing(UsingDirectiveSyntax node)
        {
            Lookup.Add($"{node.Name}");
            Resolver.AddLookupNamespace($"{node.Name}");
        }

        private void AddEnum(EnumDeclarationSyntax node)
        {
            var id = $"{node.Identifier}";

            Klass = new EnumClass(this, id);
            Ctx.Types.Add(id, Klass);
        }
        private void AddEnumMember(EnumMemberDeclarationSyntax node)
        {
            var value = ((EnumClass)Klass).AddMember(node);
            Globals.SetStaticField(
                Klass, node.Identifier.Text, HybInstance.Int(value));
        }

        private void AddClass(ClassDeclarationSyntax node)
        {
            var id = $"{node.Identifier}";

            if (node.Modifiers.Contains("partial"))
                throw new SemanticViolationException($"partial keyword is not supported: {id}");
            if (Ctx.Types.ContainsKey(id))
                throw new SemanticViolationException($"Class redefination is not supported: {id}");

            HybType parentType = null;
            var interfaceTypes = new List<HybType>();
            if (node.BaseList != null)
            {
                foreach (var b in node.BaseList.Types)
                {
                    var type = Resolver.GetType($"{b.Type}");
                    if (type.IsInterface == false)
                    {
                        if (parentType != null)
                            throw new SemanticViolationException($"Cannot be derived from more than 2 base classes.");
                        if (type.IsSealed)
                            throw new SemanticViolationException($"Sealed class cannot be inherited.");

                        parentType = type;
                    }
                    else
                    {
                        interfaceTypes.Add(type);
                    }
                }
            }

            Klass = new Class(this, id, parentType, interfaceTypes.ToArray());
            Ctx.Types.Add(id, Klass);
        }
        private void AddIndexer(IndexerDeclarationSyntax node)
        {
            var type = Resolver.GetType($"{node.Type}");
            var propertyInfo = Klass.AddProperty("[]", node);
        }
        private void AddProperty(PropertyDeclarationSyntax node)
        {
            var isStatic = node.Modifiers.IsStatic();
            var type = Resolver.GetType($"{node.Type}");
            var id = $"{node.Identifier}";

            var propertyInfo = Klass.AddProperty(id, node);

            if (propertyInfo.IsStatic)
                InitializeStaticProperty(propertyInfo);
        }
        private void AddField(FieldDeclarationSyntax node)
        {
            var type = Resolver.GetType($"{node.Declaration.Type}");

            foreach (var f in node.Declaration.Variables)
            {
                var id = $"{f.Identifier}";

                var fieldInfo = Klass.AddField(id, node, f);
                if (fieldInfo.IsStatic)
                    InitializeStaticField(f, id, type);
            }
        }

        private void InitializeStaticProperty(SSInterpretPropertyInfo info)
        {
            if (info.HasBackingField == false)
                return;

            var backingField = info.BackingField;

            if (info.Initializer == null)
                Globals.SetStaticField(Klass, backingField.Id, info.Type.GetDefault());
            else
            {
                var capturedKlass = Klass;
                AddLazyInitializer(() =>
                {
                    Globals.SetStaticField(
                        capturedKlass,
                        backingField.Id, RunExpression(info.Initializer.Value));
                });
            }
        }
        private void InitializeStaticField(VariableDeclaratorSyntax field, string id, HybType type)
        {
            if (field.Initializer == null)
                Globals.SetStaticField(Klass, id, type.GetDefault());
            else
            {
                var capturedKlass = Klass;
                AddLazyInitializer(() =>
                {
                    Globals.SetStaticField(
                        capturedKlass,
                        id, RunExpression(field.Initializer.Value));
                });
            }
        }

        private void AddConstructorMethod(ConstructorDeclarationSyntax node)
        {
            var methodInfo = Klass.AddMethod(
                "$_ctor", node,
                BuildJumps(node.Body));

            if (methodInfo.IsStatic)
                staticInitializers[Klass] = methodInfo;
        }
        private void AddMethod(MethodDeclarationSyntax node)
        {
            Klass.AddMethod(
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
                        Label = lb.Identifier.Text,
                        Statement = (StatementSyntax)node,
                        Pc = i,
                        FrameDepth = depth
                    });
                }

                FindJumpsDownwards(child, jumps, depth + 1);
            }
        }
    }
}
