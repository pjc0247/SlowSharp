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
        public HybType Type { get; protected set; }

        public SSMethodInfo SetMethod { get; protected set; }
        public SSMethodInfo GetMethod { get; protected set; }

        //public abstract HybInstance GetValue(HybInstance _this);
        //public abstract HybInstance SetValue(HybInstance _this, HybInstance value);
    }

    public class SSCompiledPropertyInfo : SSPropertyInfo
    {
        internal SSCompiledPropertyInfo(PropertyInfo property)
        {
            this.Type = HybTypeCache.GetHybType(property.PropertyType);
            this.GetMethod = property.CanRead ? new SSMethodInfo(property.GetMethod) : null;
            this.SetMethod = property.CanWrite ? new SSMethodInfo(property.SetMethod) : null;
        }
    }
    public class SSInterpretPropertyInfo : SSPropertyInfo
    {
        public BasePropertyDeclarationSyntax Property { get; }
        public EqualsValueClauseSyntax Initializer
        {
            get
            {
                if (Property is PropertyDeclarationSyntax pd)
                    return pd.Initializer;
                return null;
            }
        }

        internal SSFieldInfo BackingField { get; private set; }
        internal bool HasBackingField => BackingField != null;

        internal SSInterpretPropertyInfo(Class klass, Runner runner, string id, BasePropertyDeclarationSyntax node)
        {
            this.Id = id;
            this.Property = node;
            this.DeclaringClass = klass;
            this.Type = runner.Resolver.GetType($"{node.Type}");

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
            GetMethod = new SSMethodInfo(
                runner, $"get_{Id}", DeclaringType, invokable,
                new HybType[] { }, Type);
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
                        var vf = new VarFrame(runner.Vars);
                        if (node is IndexerDeclarationSyntax id)
                        {
                            var cnt = 0;
                            foreach (var p in id.ParameterList.Parameters)
                                vf.SetValue(p.Identifier.Text, args.ElementAt(cnt++));
                        }

                        runner.RunBlock(get.Body, vf);
                        return runner.Ret;
                    }
                    else
                    {
                        if (IsStatic)
                            return runner.Globals.GetStaticField(DeclaringClass, BackingField.Id);

                        HybInstance value;
                        if (runner.Ctx._this.GetPropertyOrField($"__{this.Id}", out value, AccessLevel.This))
                            return value;
                        throw new InvalidOperationException();
                    }
                });
                GetMethod = new SSMethodInfo(
                    runner, $"get_{Id}", DeclaringType, invokable,
                    new HybType[] { }, Type);
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
                        var vf = new VarFrame(runner.Vars);
                        vf.SetValue("value", args[1]);

                        if (node is IndexerDeclarationSyntax id)
                        {
                            var cnt = 0;
                            foreach (var p in id.ParameterList.Parameters)
                                vf.SetValue(p.Identifier.Text, args.ElementAt(cnt++));
                        }

                        runner.RunBlock(set.Body, vf);
                        return runner.Ret;
                    }
                    else
                    {
                        if (IsStatic)
                        {
                            runner.Globals.SetStaticField(DeclaringClass, BackingField.Id, args[0]);
                            return null;
                        }

                        if (runner.Ctx._this.SetPropertyOrField($"__{this.Id}", args[0], AccessLevel.This))
                            return null;
                        throw new InvalidOperationException();
                    }
                });
                SetMethod = new SSMethodInfo(
                    runner, $"set_{Id}", DeclaringType, invokable,
                    new HybType[] { Type }, HybTypeCache.Void);
            }
        }

        private void AddBackingFieldIfNotExist(Runner runner, BasePropertyDeclarationSyntax node)
        {
            var id = $"__{this.Id}";
            if (DeclaringClass.HasField(id))
                return;

            BackingField = new SSInterpretFieldInfo(DeclaringClass)
            {
                Id = id,
                fieldType = runner.Resolver.GetType($"{node.Type}"),
                AccessModifier = AccessModifier.Private
            };
            DeclaringClass.AddField(BackingField);
        }
    }
}
