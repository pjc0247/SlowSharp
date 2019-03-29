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

        public bool IsSafeType(HybType type)
        {
            // Interpret type is always safe
            if (type.isCompiledType == false)
                return true;

            var ct = type.compiledType;
            if (IsBlockedNamespace(ct.Namespace))
                return false;
            return klassFilters.Contains(
                ct.FullName) == false;
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
