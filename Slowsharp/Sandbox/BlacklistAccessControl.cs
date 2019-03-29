using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class BlacklistAccessControl : IAccessFilter
    {
        public static BlacklistAccessControl Default
        {
            get
            {
                var ac = new BlacklistAccessControl();
                ac.AddDefaultPolicies();
                return ac;
            }
        }

        private HashSet<string> namespaceFilters = new HashSet<string>();
        private HashSet<string> klassFilters = new HashSet<string>();

        public void AddBlockedType(string id)
        {
            klassFilters.Add(id);
        }
        public void AddBlockedNamespace(string ns)
        {
            namespaceFilters.Add(ns);
        }
        public void AddDefaultPolicies()
        {
            namespaceFilters.Add("System.IO");
            namespaceFilters.Add("System.Threading");
            namespaceFilters.Add("System.Reflection");
            namespaceFilters.Add("System.Diagnostics");
            namespaceFilters.Add("System.Net");
            klassFilters.Add("System.AppDomain");
            klassFilters.Add("System.Environment");
        }

        public bool IsSafeType(HybType type)
        {
            // Interpret type is always safe
            if (type.isCompiledType == false)
                return true;

            var ct = type.compiledType;
            if (IsAllowedNamespace(ct.Namespace) == false)
                return false;
            return klassFilters.Contains(
                ct.FullName) == false;
        }
        public bool IsAllowedNamespace(string ns)
        {
            foreach (var filter in namespaceFilters)
            {
                if (ns.StartsWith(filter))
                    return false;
            }
            return true;
        }
    }
}
