using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public class SSPropertyInfo : SSMemberInfo
    {
        public PropertyDeclarationSyntax property;
        public HybType type { get; }

        public Invokable setMethod { get; private set; }
        public Invokable getMethod { get; private set; }

        internal SSFieldInfo backingField { get; private set; }
        internal bool hasBackingField => backingField != null;

        internal SSPropertyInfo(PropertyInfo property)
        {
            this.type = new HybType(property.PropertyType);
            this.getMethod = property.CanRead ? new Invokable(property.GetMethod) : null;
            this.setMethod = property.CanWrite ? new Invokable(property.SetMethod) : null;
        }
        internal SSPropertyInfo(Class klass, Runner runner, PropertyDeclarationSyntax node)
        {
            this.property = node;
            this.declaringClass = klass;
            this.type = runner.resolver.GetType($"{node.Type}");

            if (node.ExpressionBody != null)
                InitializeWithExpressionBody(runner, node);
            else InitializeWithAccessor(runner, node);
        }

        private void InitializeWithExpressionBody(Runner runner, PropertyDeclarationSyntax node)
        {
            getMethod = new Invokable((args) =>
            {
                return runner.RunExpression(node.ExpressionBody.Expression);
            });
        }
        private void InitializeWithAccessor(Runner runner, PropertyDeclarationSyntax node)
        {
            var accs = node.AccessorList.Accessors;

            var get = accs.Where(x => x.Keyword.Text == "get")
                .FirstOrDefault();
            if (get != null)
            {
                if (get.ExpressionBody == null && get.Body == null)
                    AddBackingFieldIfNotExist(runner, node);

                getMethod = new Invokable((args) =>
                {
                    if (get.ExpressionBody != null)
                        return runner.RunExpression(get.ExpressionBody.Expression);
                    else if (get.Body != null)
                    {
                        runner.RunBlock(get.Body);
                        return runner.ret;
                    }
                    else
                    {
                        if (isStatic)
                            return runner.globals.GetStaticField(declaringClass, backingField.id);

                        HybInstance value;
                        if (runner.ctx._this.GetPropertyOrField($"__{node.Identifier}", out value, AccessLevel.This))
                            return value;
                        throw new InvalidOperationException();
                    }
                });
            }

            var set = accs.Where(x => x.Keyword.Text == "set")
                .FirstOrDefault();
            if (set != null)
            {
                if (set.ExpressionBody == null && set.Body == null)
                    AddBackingFieldIfNotExist(runner, node);

                setMethod = new Invokable((args) =>
                {
                    if (set.ExpressionBody != null)
                        return runner.RunExpression(set.ExpressionBody.Expression);
                    else if (set.Body != null)
                    {
                        runner.RunBlock(set.Body);
                        return runner.ret;
                    }
                    else
                    {
                        if (isStatic)
                        {
                            runner.globals.SetStaticField(declaringClass, backingField.id, args[0]);
                            return null;
                        }

                        if (runner.ctx._this.SetPropertyOrField($"__{node.Identifier}", args[0], AccessLevel.This))
                            return null;
                        throw new InvalidOperationException();
                    }
                });
            }
        }

        private void AddBackingFieldIfNotExist(Runner runner, PropertyDeclarationSyntax node)
        {
            var id = $"__{node.Identifier}";
            if (declaringClass.HasField(id))
                return;

            backingField = new SSInterpretFieldInfo(declaringClass)
            {
                id = id,
                fieldType = runner.resolver.GetType($"{node.Type}"),
                accessModifier = AccessModifier.Private
            };
            declaringClass.AddField(backingField);
        }
    }
}
