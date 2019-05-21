using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp.JSON
{
    public class JsonConvert
    {
        public static string SerializeObject(HybInstance obj)
        {
            var sb = new StringBuilder();
            _SerializeObject(sb, obj);
            return sb.ToString();
        }
        private static void _SerializeObject(StringBuilder sb, HybInstance obj)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            var type = obj.GetHybType();
            if (type.IsPrimitive)
            {
                if (type.CompiledType == typeof(string))
                    sb.Append("\"");
                sb.Append(obj.InnerObject.ToString());
                if (type.CompiledType == typeof(string))
                    sb.Append("\"");
                return;
            }

            if (type.IsArray)
            {
                sb.Append("[");

                sb.Append("]");
            }
        }
    }
}
