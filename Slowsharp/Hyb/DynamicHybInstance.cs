using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    public class DynamicHybInstance : DynamicObject
    {
        private HybInstance Obj;

        public DynamicHybInstance(HybInstance obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            this.Obj = obj;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Obj.Invoke(binder.Name, args);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            HybInstance value;
            if (Obj.GetIndexer(indexes, out value))
            {
                result = value.Unwrap();
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            HybInstance value;
            if (Obj.GetPropertyOrField(binder.Name, out value))
            {
                result = value.Unwrap();
                return true;
            }
            throw new ArgumentException($"No such member: {binder.Name}");
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Obj.SetPropertyOrField(binder.Name, value.Wrap()))
                return true;
            throw new ArgumentException($"No such member: {binder.Name}");
        }
    }
}
