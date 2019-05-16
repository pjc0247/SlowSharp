using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public abstract class SSPropertyInfo : SSMemberInfo
    {
        public HybType type { get; protected set; }

        public SSMethodInfo setMethod { get; protected set; }
        public SSMethodInfo getMethod { get; protected set; }

        //public abstract HybInstance GetValue(HybInstance _this);
        //public abstract HybInstance SetValue(HybInstance _this, HybInstance value);
    }

    public class SSCompiledPropertyInfo : SSPropertyInfo
    {
        internal SSCompiledPropertyInfo(PropertyInfo property)
        {
            this.type = HybTypeCache.GetHybType(property.PropertyType);
            this.getMethod = property.CanRead ? new SSMethodInfo(property.GetMethod) : null;
            this.setMethod = property.CanWrite ? new SSMethodInfo(property.SetMethod) : null;
        }
    }
    public class SSInterpretPropertyInfo : SSPropertyInfo
    {
        public BasePropertyDeclarationSyntax property { get; }
        public EqualsValueClauseSyntax initializer
        {
            get
            {
                if (property is PropertyDeclarationSyntax pd)
                    return pd.Initializer;
                return null;
            }
        }

        internal SSFieldInfo backingField { get; private set; }
        internal bool hasBackingField => backingField != null;

        internal SSInterpretPropertyInfo(Class klass, Runner runner, string id, BasePropertyDeclarationSyntax node)
        {
            this.id = id;
            this.property = node;
            this.declaringClass = klass;
            this.type = runner.resolver.GetType($"{node.Type}");

            if (node is PropertyDeclarationSyntax pd &&
                pd.ExpressionBody != null)
                InitializeWithExpressionBody(runner, pd);
            else InitializeWithAccessor(runner, node);
        }

        private void InitializeWithExpressionBody(Runner runner, PropertyDeclarationSyntax node)
        {
            var invokable = new Invokable(runner, (args) =>
            {
                return runner.RunExpression(node.ExpressionBody.Expression);
            });
            getMethod = new SSMethodInfo(
                runner, $"get_{id}", declaringType, invokable,
                new HybType[] { }, type);
        }
        private void InitializeWithAccessor(Runner runner, BasePropertyDeclarationSyntax node)
        {
            var accs = node.AccessorList.Accessors;

            var get = accs.Where(x => x.Keyword.Text == "get")
                .FirstOrDefault();
            if (get != null)
            {
                if (get.ExpressionBody == null && get.Body == null)
                    AddBackingFieldIfNotExist(runner, node);
                
                var invokable = new Invokable(runner, (args) =>
                {
                    if (get.ExpressionBody != null)
                        return runner.RunExpression(get.ExpressionBody.Expression);
                    else if (get.Body != null)
                    {
                        var vf = new VarFrame(runner.vars);
                        if (node is IndexerDeclarationSyntax id)
                        {
                            var cnt = 0;
                            foreach (var p in id.ParameterList.Parameters)
                                vf.SetValue(p.Identifier.Text, args.ElementAt(cnt++));
                        }

                        runner.RunBlock(get.Body, vf);
                        return runner.ret;
                    }
                    else
                    {
                        if (isStatic)
                            return runner.globals.GetStaticField(declaringClass, backingField.id);

                        HybInstance value;
                        if (runner.ctx._this.GetPropertyOrField($"__{this.id}", out value, AccessLevel.This))
                            return value;
                        throw new InvalidOperationException();
                    }
                });
                getMethod = new SSMethodInfo(
                    runner, $"get_{id}", declaringType, invokable,
                    new HybType[] { }, type);
            }

            var set = accs.Where(x => x.Keyword.Text == "set")
                .FirstOrDefault();
            if (set != null)
            {
                if (set.ExpressionBody == null && set.Body == null)
                    AddBackingFieldIfNotExist(runner, node);

                var invokable = new Invokable(runner, (args) =>
                {
                    if (set.ExpressionBody != null)
                        return runner.RunExpression(set.ExpressionBody.Expression);
                    else if (set.Body != null)
                    {
                        var vf = new VarFrame(runner.vars);
                        vf.SetValue("value", args[1]);

                        if (node is IndexerDeclarationSyntax id)
                        {
                            var cnt = 0;
                            foreach (var p in id.ParameterList.Parameters)
                                vf.SetValue(p.Identifier.Text, args.ElementAt(cnt++));
                        }

                        runner.RunBlock(set.Body, vf);
                        return runner.ret;
                    }
                    else
                    {
                        if (isStatic)
                        {
                            runner.globals.SetStaticField(declaringClass, backingField.id, args[0]);
                            return null;
                        }

                        if (runner.ctx._this.SetPropertyOrField($"__{this.id}", args[0], AccessLevel.This))
                            return null;
                        throw new InvalidOperationException();
                    }
                });
                setMethod = new SSMethodInfo(
                    runner, $"set_{id}", declaringType, invokable,
                    new HybType[] { type }, HybTypeCache.Void);
            }
        }

        private void AddBackingFieldIfNotExist(Runner runner, BasePropertyDeclarationSyntax node)
        {
            var id = $"__{this.id}";
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
