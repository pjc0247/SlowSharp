﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    public partial class Runner
    {
        private HybInstance RunBinaryExpression(BinaryExpressionSyntax node)
        {
            var op = node.OperatorToken.ValueText;
            if (op == "is")
                return RunIs(node);

            var left = RunExpression(node.Left);

            if (op == "||") {
                if (IsTrueOrEquivalent(left))
                    return HybInstance.Bool(true);
            }
            else if (op == "&&")
            {
                if (IsTrueOrEquivalent(left) == false)
                    return HybInstance.Bool(false);
            }

            var right = RunExpression(node.Right);

            return MadMath.Op(left, right, op);

            if (left.Is<string>())
                return HybInstance.String(left.As<string>() + right.As<string>());

            var decLeft = Convert.ToDecimal(left.innerObject);
            var decRight = Convert.ToDecimal(right.innerObject);
            decimal result = 0;

            if (op == "+") result = decLeft + decRight;
            else if (op == "-") result = decLeft - decRight;
            else if (op == "*") result = decLeft * decRight;
            else if (op == "/") result = decLeft / decRight;
            else if (op == "%") result = decLeft % decRight;
            else if (op == ">") return HybInstance.Bool(decLeft > decRight);
            else if (op == ">=") return HybInstance.Bool(decLeft >= decRight);
            else if (op == "<") return HybInstance.Bool(decLeft < decRight);
            else if (op == "<=") return HybInstance.Bool(decLeft <= decRight);
            else if (op == "==") return HybInstance.Bool(decLeft == decRight);

            //if (left.Is<Int16>()) return Convert.ToInt16(result);
            else if (left.Is<Int32>()) return HybInstance.Int(Convert.ToInt32(result));
            /*
            else if (left.Is<Int64>()) return Convert.ToInt64(result);
            else if (left.Is<UInt16>()) return Convert.ToUInt16(result);
            else if (left.Is<UInt32>()) return Convert.ToUInt32(result);
            else if (left.Is<UInt64>()) return Convert.ToUInt64(result);
            else if (left.Is<float>()) return Convert.ToSingle(result);
            else if (left.Is<double>()) return Convert.ToDouble(result);
            else if (left.Is<decimal>()) return Convert.ToDecimal(result);
            */

            return null;
        }

        private HybInstance RunIs(BinaryExpressionSyntax node)
        {
            var left = RunExpression(node.Left);
            var type = resolver.GetType($"{node.Right}");

            if (type == null)
                throw new SemanticViolationException($"Unrecognized type: {node.Right}");

            return left.Is(type) ? HybInstance.Bool(true) : HybInstance.Bool(false);
        }
    }
}
