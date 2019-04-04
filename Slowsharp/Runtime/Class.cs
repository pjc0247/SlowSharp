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
        public string id { get; }

        public HybType parent { get; }
        public HybType type { get; }

        internal Runner runner;

        //private Dictionary<string, object> properties = new Dictionary<string, object>();
        private Dictionary<string, SSFieldInfo> fields = new Dictionary<string, SSFieldInfo>();
        private Dictionary<string, SSPropertyInfo> properties = new Dictionary<string, SSPropertyInfo>();
        private Dictionary<string, List<SSMethodInfo>> methods = new Dictionary<string, List<SSMethodInfo>>();

        public Class(Runner runner, string id)
        {
            this.runner = runner;
            this.id = id;
            this.type = new HybType(this);
        }
        public Class(Runner runner, string id, HybType parent) :
            this(runner, id)
        {
            this.parent = parent;

            if (parent != null)
                InheritFrom(parent);
        }

        private void InheritFrom(HybType parent)
        {
            if (parent.isSealed)
                throw new SemanticViolationException($"Sealed class cannot be inherited.");

            if (parent.isCompiledType)
            {
                foreach (var m in parent.GetMethods())
                {
                    if (m.accessModifier == AccessModifier.Private)
                        continue;

                    if (methods.ContainsKey(m.id) == false)
                        methods[m.id] = new List<SSMethodInfo>();
                    methods[m.id].Add(m);
                }
            }
            else
            {
                var klass = parent.interpretKlass;
                foreach (var f in klass.fields
                    .Where(x => x.Value.accessModifier != AccessModifier.Private))
                    fields.Add(f.Key, f.Value);
                foreach (var m in klass.methods)
                {
                    var nonPrivateMethods = m.Value
                        .Where(x => x.accessModifier != AccessModifier.Private)
                        .ToList();
                    methods.Add(m.Key, nonPrivateMethods);
                }
            }
        }

        private void EnsureMethodKey(string id)
        {
            if (methods.ContainsKey(id) == false)
                methods[id] = new List<SSMethodInfo>();
        }
        public void AddMethod(string id, BaseMethodDeclarationSyntax method, JumpDestination[] jumps)
        {
            EnsureMethodKey(id);

            methods[id].Add(new SSMethodInfo(runner, type, method) {
                id = id,
                isStatic = method.Modifiers.IsStatic(),
                declaringClass = this,
                declaration = method,
                jumps = jumps,

                accessModifier = AccessModifierParser.Parse(method.Modifiers)
            });
        }
        public SSPropertyInfo AddProperty(string id, PropertyDeclarationSyntax property)
        {
            var propertyInfo = new SSPropertyInfo(this, runner, property)
            {
                id = id,
                isStatic = property.Modifiers.IsStatic(),
                property = property,
                declaringClass = this,

                accessModifier = AccessModifierParser.Parse(property.Modifiers)
            };
            properties.Add(id, propertyInfo);
            return propertyInfo;
        }
        public SSFieldInfo AddField(string id, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            var fieldInfo = new SSFieldInfo()
            {
                id = id,
                fieldType = runner.resolver.GetType($"{field.Declaration.Type}"),
                isStatic = field.Modifiers.IsStatic(),
                field = field,
                declaringClass = this,
                declartor = declarator,

                accessModifier = AccessModifierParser.Parse(field.Modifiers)
            };
            fields.Add(id, fieldInfo);
            return fieldInfo;
        }
        public void AddField(SSFieldInfo field)
        {
            fields.Add(field.id, field);
        }

        public SSMethodInfo[] GetMethods()
        {
            return methods.SelectMany(x => x.Value).ToArray();
        }
        public SSMethodInfo[] GetMethods(string id)
        {
            if (methods.ContainsKey(id) == false)
                return new SSMethodInfo[] { };

            return methods[id].ToArray();
        }
        public SSFieldInfo[] GetFields()
        {
            return fields
                .Select(x => x.Value)
                .ToArray();
        }
        public SSPropertyInfo[] GetProperties()
        {
            return properties
                .Select(x => x.Value)
                .ToArray();
        }

        public bool HasStaticField(string id)
        {
            if (fields.ContainsKey(id) == false)
                return false;
            return fields[id].isStatic == true;
        }
        public bool HasField(string id, MemberFlag flag = MemberFlag.None)
        {
            if (fields.ContainsKey(id) == false)
                return false;

            var field = fields[id];
            return field.IsMatch(flag);
        }
        public bool TryGetField(string id, out SSFieldInfo field)
        {
            field = null;
            if (HasField(id) == false)
                return false;
            field = fields[id];
            return true;
        }
        public SSFieldInfo GetField(string id)
        {
            if (HasField(id) == false)
                throw new ArgumentException($"No such field: {id}");
            return fields[id];
        }

        public bool HasStaticProperty(string id)
        {
            if (properties.ContainsKey(id) == false)
                return false;
            return properties[id].isStatic == true;
        }
        public bool HasProperty(string id, MemberFlag flag = MemberFlag.None)
        {
            if (properties.ContainsKey(id) == false)
                return false;

            var property = properties[id];
            return property.IsMatch(flag);
        }
        public bool TryGetProperty(string id, out SSPropertyInfo property)
        {
            property = null;
            if (HasProperty(id) == false)
                return false;
            property = properties[id];
            return true;
        }
        public SSPropertyInfo GetProperty(string id)
        {
            if (HasProperty(id) == false)
                throw new ArgumentException($"No such property: {id}");
            return properties[id];
        }
    }
}
