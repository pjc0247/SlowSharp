using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slowsharp
{
    /// <summary>
    /// SlowSharpFieldInfo
    /// </summary>
    public abstract class SSFieldInfo : SSMemberInfo
    {
        public HybType fieldType;

        public abstract HybInstance GetValue(HybInstance _this);
    }

    public class SSCompiledFieldInfo : SSFieldInfo
    {
        internal FieldInfo fieldInfo;

        internal SSCompiledFieldInfo(FieldInfo field)
        {
            this.origin = SSMemberOrigin.InterpretScript;
            this.fieldType = new HybType(field.FieldType);
            this.fieldInfo = field;
        }

        public override HybInstance GetValue(HybInstance _this)
            => HybInstance.Object(fieldInfo.GetValue(_this.Unwrap()));
    }
    public class SSInterpretFieldInfo : SSFieldInfo
    {
        internal VariableDeclaratorSyntax declartor;
        internal FieldDeclarationSyntax field;

        private int fieldPtr = -1;

        internal SSInterpretFieldInfo()
        {
        }

        public override HybInstance GetValue(HybInstance _this)
        {
            if (isStatic)
            {
                return declaringClass.runner.globals
                    .GetStaticField(declaringClass, id);
            }

            // it's fast enough to do like this
            if (fieldPtr == -1)
                fieldPtr = _this.fields.GetPtr(id);
            return _this.fields.GetByPtr(fieldPtr);
        }
    }
}
