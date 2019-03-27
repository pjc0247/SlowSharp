using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class AccessControl
    {
        private HashSet<string> namespaceFilters = new HashSet<string>();
        private HashSet<string> klassFilters = new HashSet<string>();

        public void AddDefaultPolicies()
        {
            namespaceFilters.Add("System.IO");
            namespaceFilters.Add("System.Threading");
            namespaceFilters.Add("System.Reflection");
            namespaceFilters.Add("System.Diagnostics");
            klassFilters.Add("System.AppDomain");
        }

        public bool IsSafeType(Type type)
        {
            return klassFilters.Contains(type.FullName) == false;
        }
        public bool IsBlockedNamespace(string ns)
        {
            foreach (var filter in namespaceFilters)
            {
                if (ns.StartsWith(filter))
                    return true;
            }
            return false;
        }
    }
}
