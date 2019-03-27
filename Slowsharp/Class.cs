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
        //private Dictionary<string, object> properties = new Dictionary<string, object>();
        private Dictionary<string, SSFieldInfo> fields = new Dictionary<string, SSFieldInfo>();
        private Dictionary<string, Invokable> methods = new Dictionary<string, Invokable>();
        private Runner runner;

        public Class(Runner runner)
        {
            this.runner = runner;
        }

        public void AddMethod(string id, BaseMethodDeclarationSyntax method)
        {
            methods.Add(id, new Invokable(runner, method));
        }
        public void AddField(string id, FieldDeclarationSyntax field, VariableDeclaratorSyntax declarator)
        {
            fields.Add(id, new SSFieldInfo()
            {
                id = id,
                field = field,
                declartor = declarator
            });
        }

        public Invokable[] GetMethods(string id)
        {
            return methods
                .Where(x => x.Key == id)
                .Select(x => x.Value)
                .ToArray();
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
        /*
        public Invokable[] GetMethods(string id)
        {
            return new Invokable[] { methods[id] };
        }
        */
    }
}
