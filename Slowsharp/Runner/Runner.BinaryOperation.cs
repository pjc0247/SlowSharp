using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private object RunBinaryExpression(BinaryExpressionSyntax node)
        {
            var op = node.OperatorToken.ValueText;
            var left = RunExpression(node.Left);
            var right = RunExpression(node.Right);

            if (left is string)
                return (string)left + (string)right;

            var decLeft = Convert.ToDecimal(left);
            var decRight = Convert.ToDecimal(right);
            decimal result = 0;

            if (op == "+") result = decLeft + decRight;
            else if (op == "-") result = decLeft - decRight;
            else if (op == "*") result = decLeft * decRight;
            else if (op == "/") result = decLeft / decRight;
            else if (op == "%") result = decLeft % decRight;
            else if (op == ">") return decLeft > decRight;
            else if (op == ">=") return decLeft > decRight;
            else if (op == "<") return decLeft < decRight;
            else if (op == "<=") return decLeft <= decRight;

            if (left is Int16) return Convert.ToInt16(result);
            else if (left is Int32) return Convert.ToInt32(result);
            else if (left is Int64) return Convert.ToInt64(result);
            else if (left is UInt16) return Convert.ToUInt16(result);
            else if (left is UInt32) return Convert.ToUInt32(result);
            else if (left is UInt64) return Convert.ToUInt64(result);
            else if (left is float) return Convert.ToSingle(result);
            else if (left is double) return Convert.ToDouble(result);
            else if (left is decimal) return Convert.ToDecimal(result);

            return null;
        }
    }
}
