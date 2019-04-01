﻿using System;
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

        //private Dictionary<string, object> properties = new Dictionary<string, object>();
        private Dictionary<string, SSFieldInfo> fields = new Dictionary<string, SSFieldInfo>();
        private Dictionary<string, SSPropertyInfo> properties = new Dictionary<string, SSPropertyInfo>();
        private Dictionary<string, List<SSMethodInfo>> methods = new Dictionary<string, List<SSMethodInfo>>();
        private Runner runner;

        public Class(Runner runner, string id)
        {
            this.runner = runner;
            this.id = id;
        }
        public Class(Runner runner, string id, HybType parent)
        {
            this.runner = runner;
            this.id = id;
            this.parent = parent;

            InheritFrom(parent);
        }

        private void InheritFrom(HybType parent)
        {
            if (parent.isCompiledType)
            {
            }
            else
            {
                var klass = parent.interpretKlass;
                foreach (var f in klass.fields)
                    fields.Add(f.Key, f.Value);
                foreach (var m in klass.methods)
                    methods.Add(m.Key, m.Value);
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

            methods[id].Add(new SSMethodInfo(runner, method) {
                id = id,
                isStatic = method.Modifiers.IsStatic(),
                declaringClass = this,
                declaration = method,
                jumps = jumps,

                accessModifier = AccessModifierParser.Parse(method.Modifiers)
            });
        }
        public void AddProperty(string id, PropertyDeclarationSyntax property)
        {
            properties.Add(id, new SSPropertyInfo(this, runner, property)
            {
                id = id,
                isStatic = property.Modifiers.IsStatic(),
                property = property,
                declaringClass = this,

                accessModifier = AccessModifierParser.Parse(property.Modifiers)
            });
        }
        public void AddField(string id, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            fields.Add(id, new SSFieldInfo()
            {
                id = id,
                fieldType = runner.resolver.GetType($"{field.Declaration.Type}"),
                isStatic = field.Modifiers.IsStatic(),
                field = field,
                declaringClass = this,
                declartor = declarator,

                accessModifier = AccessModifierParser.Parse(field.Modifiers)
            });
        }
        public void AddField(SSFieldInfo field)
        {
            fields.Add(field.id, field);
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

        public bool HasField(string id)
        {
            return fields.ContainsKey(id);
        }
        public SSFieldInfo GetField(string id)
        {
            if (HasField(id) == false)
                throw new ArgumentException($"No such field: {id}");
            return fields[id];
        }

        public bool HasProperty(string id)
        {
            return properties.ContainsKey(id);
        }
        public SSPropertyInfo GetProperty(string id)
        {
            if (HasProperty(id) == false)
                throw new ArgumentException($"No such property: {id}");
            return properties[id];
        }
    }
}
