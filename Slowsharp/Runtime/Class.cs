using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    internal class Class
    {
        public string Id { get; }

        public HybType Parent { get; }
        public HybType[] Interfaces { get; }
        public HybType Type { get; }

        internal Runner Runner;

        //private Dictionary<string, object> properties = new Dictionary<string, object>();
        private Dictionary<string, SSFieldInfo> Fields = new Dictionary<string, SSFieldInfo>();
        private Dictionary<string, SSPropertyInfo> Properties = new Dictionary<string, SSPropertyInfo>();
        private Dictionary<string, List<SSMethodInfo>> Methods = new Dictionary<string, List<SSMethodInfo>>();

        public Class(Runner runner, string id)
        {
            this.Runner = runner;
            this.Id = id;
            this.Type = new HybType(this);
        }
        public Class(Runner runner, string id, HybType parent, HybType[] interfaces) :
            this(runner, id)
        {
            this.Parent = parent;
            this.Interfaces = interfaces;

            if (parent != null)
                InheritFrom(parent);
        }

        private void InheritFrom(HybType parent)
        {
            if (parent.IsSealed)
                throw new SemanticViolationException($"Sealed class cannot be inherited.");

            if (parent.IsCompiledType)
            {
                foreach (var m in parent.GetMethods())
                {
                    if (m.AccessModifier == AccessModifier.Private)
                        continue;

                    if (Methods.ContainsKey(m.Id) == false)
                        Methods[m.Id] = new List<SSMethodInfo>();
                    Methods[m.Id].Add(m);
                }
            }
            else
            {
                var klass = parent.InterpretKlass;
                foreach (var f in klass.Fields
                    .Where(x => x.Value.AccessModifier != AccessModifier.Private))
                    Fields.Add(f.Key, f.Value);
                foreach (var m in klass.Methods)
                {
                    var nonPrivateMethods = m.Value
                        .Where(x => x.AccessModifier != AccessModifier.Private)
                        .ToList();
                    Methods.Add(m.Key, nonPrivateMethods);
                }
            }
        }

        private void EnsureMethodKey(string id)
        {
            if (Methods.ContainsKey(id) == false)
                Methods[id] = new List<SSMethodInfo>();
        }
        public SSMethodInfo AddMethod(string id, BaseMethodDeclarationSyntax method, JumpDestination[] jumps)
        {
            EnsureMethodKey(id);

            var signature = MemberSignature.GetSignature(
                Runner.Resolver, id, method);

            var methodInfo = new SSInterpretMethodInfo(Runner, id, Type, method)
            {
                Id = id,
                IsStatic = method.Modifiers.IsStatic(),
                DeclaringClass = this,
                Declaration = method,
                Jumps = jumps,

                AccessModifier = AccessModifierParser.Parse(method.Modifiers)
            };

            Methods[id].RemoveAll(x => x.Signature == signature);
            Methods[id].Add(methodInfo);

            return methodInfo;
        }
        public SSInterpretPropertyInfo AddProperty(string id, BasePropertyDeclarationSyntax property)
        {
            var propertyInfo = new SSInterpretPropertyInfo(this, Runner, id, property)
            {
                IsStatic = property.Modifiers.IsStatic(),
                DeclaringClass = this,

                AccessModifier = AccessModifierParser.Parse(property.Modifiers)
            };
            Properties.Add(id, propertyInfo);
            return propertyInfo;
        }
        public SSFieldInfo AddField(string id, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            var fieldInfo = new SSInterpretFieldInfo(this)
            {
                Id = id,
                fieldType = Runner.Resolver.GetType($"{field.Declaration.Type}"),
                IsStatic = field.Modifiers.IsStatic() | field.Modifiers.IsConst(),
                isConst = field.Modifiers.IsConst(),
                field = field,
                DeclaringClass = this,
                declartor = declarator,

                AccessModifier = AccessModifierParser.Parse(field.Modifiers)
            };
            Fields.Add(id, fieldInfo);
            return fieldInfo;
        }
        public void AddField(SSFieldInfo field)
        {
            Fields.Add(field.Id, field);
        }

        public SSMethodInfo[] GetMethods()
        {
            return Methods.SelectMany(x => x.Value).ToArray();
        }
        public SSMethodInfo[] GetMethods(string id)
        {
            if (Methods.ContainsKey(id) == false)
                return new SSMethodInfo[] { };

            return Methods[id].ToArray();
        }
        public SSFieldInfo[] GetFields()
        {
            return Fields
                .Select(x => x.Value)
                .ToArray();
        }
        public SSPropertyInfo[] GetProperties()
        {
            return Properties
                .Select(x => x.Value)
                .ToArray();
        }

        public bool HasStaticField(string id)
        {
            if (Fields.ContainsKey(id) == false)
                return false;
            return Fields[id].IsStatic == true;
        }
        public bool HasField(string id, MemberFlag flag = MemberFlag.None)
        {
            if (Fields.ContainsKey(id) == false)
                return false;

            var field = Fields[id];
            return field.IsMatch(flag);
        }
        public bool TryGetField(string id, out SSFieldInfo field)
        {
            field = null;
            if (HasField(id) == false)
                return false;
            field = Fields[id];
            return true;
        }
        public SSFieldInfo GetField(string id)
        {
            if (HasField(id) == false)
                throw new ArgumentException($"No such field: {id}");
            return Fields[id];
        }

        public bool HasStaticProperty(string id)
        {
            if (Properties.ContainsKey(id) == false)
                return false;
            return Properties[id].IsStatic == true;
        }
        public bool HasProperty(string id, MemberFlag flag = MemberFlag.None)
        {
            if (Properties.ContainsKey(id) == false)
                return false;

            var property = Properties[id];
            return property.IsMatch(flag);
        }
        public bool TryGetProperty(string id, out SSPropertyInfo property)
        {
            property = null;
            if (HasProperty(id) == false)
                return false;
            property = Properties[id];
            return true;
        }
        public SSPropertyInfo GetProperty(string id)
        {
            if (HasProperty(id) == false)
                throw new ArgumentException($"No such property: {id}");
            return Properties[id];
        }
    }
}
