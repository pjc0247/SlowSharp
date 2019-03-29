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

        //private Dictionary<string, object> properties = new Dictionary<string, object>();
        private Dictionary<string, SSFieldInfo> fields = new Dictionary<string, SSFieldInfo>();
        private Dictionary<string, List<SSMethodInfo>> methods = new Dictionary<string, List<SSMethodInfo>>();
        private Runner runner;

        public Class(Runner runner, string id)
        {
            this.runner = runner;
            this.id = id;
        }

        private void EnsureMethodKey(string id)
        {
            if (methods.ContainsKey(id) == false)
                methods[id] = new List<SSMethodInfo>();
        }
        public void AddMethod(string id, BaseMethodDeclarationSyntax method)
        {
            EnsureMethodKey(id);

            methods[id].Add(new SSMethodInfo()
            {
                id = id,
                method = method,
                target = new Invokable(runner, method),

                accessModifier = AccessModifierParser.Parse(method.Modifiers)
            });
        }
        public void AddField(string id, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            fields.Add(id, new SSFieldInfo()
            {
                id = id,
                field = field,
                declartor = declarator,

                accessModifier = AccessModifierParser.Parse(field.Modifiers)
            });
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
        /*
        public Invokable[] GetMethods(string id)
        {
            return new Invokable[] { methods[id] };
        }
        */
    }
}
