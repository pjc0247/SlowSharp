using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class WhitelistAccessControl : IAccessFilter
    {
        public static WhitelistAccessControl Default
        {
            get
            {
                var ac = new WhitelistAccessControl();
                ac.AddDefaultPolicies();
                return ac;
            }
        }

        private HashSet<string> namespaceFilters = new HashSet<string>();
        private HashSet<string> klassFilters = new HashSet<string>();

        public void AddType(string id)
        {
            klassFilters.Add(id);
        }
        public void AddNamespace(string ns)
        {
            namespaceFilters.Add(ns);
        }
        public void AddDefaultPolicies()
        {
            namespaceFilters.Add("System");
        }

        public bool IsSafeType(HybType type)
        {
            // Interpret type is always safe
            if (type.IsCompiledType == false)
                return true;

            var ct = type.CompiledType;
            if (IsAllowedNamespace(ct.Namespace) == false)
                return false;
            return klassFilters.Contains(ct.FullName);
        }
        public bool IsAllowedNamespace(string ns)
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
