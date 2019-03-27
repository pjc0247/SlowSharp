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
        private HashSet<string> fields = new HashSet<string>();
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
        public void AddField(string id, FieldDeclarationSyntax field)
        {
            fields.Add(id);
        }

        public Invokable[] GetMethods(string id)
        {
            return methods
                .Where(x => x.Key == id)
                .Select(x => x.Value)
                .ToArray();
        }

        public bool HasField(string id)
        {
            return fields.Contains(id);
        }
        /*
        public Invokable[] GetMethods(string id)
        {
            return new Invokable[] { methods[id] };
        }
        */
    }
}
