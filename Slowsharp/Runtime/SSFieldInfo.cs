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
        public abstract void SetValue(HybInstance _this, HybInstance value);
    }

    public class SSCompiledFieldInfo : SSFieldInfo
    {
        internal FieldInfo fieldInfo;

        internal SSCompiledFieldInfo(FieldInfo field)
        {
            this.origin = SSMemberOrigin.InterpretScript;
            this.fieldType = HybTypeCache.GetHybType(field.FieldType);
            this.fieldInfo = field;

            this.isStatic = field.IsStatic;
        }

        public override HybInstance GetValue(HybInstance _this)
            => HybInstance.Object(fieldInfo.GetValue(isStatic ? null : _this.Unwrap()));
        public override void SetValue(HybInstance _this, HybInstance value)
            => fieldInfo.SetValue(isStatic ? null : _this.Unwrap(), value);
    }
    public class SSInterpretFieldInfo : SSFieldInfo
    {
        internal VariableDeclaratorSyntax declartor;
        internal FieldDeclarationSyntax field;

        private int fieldPtr = -1;

        internal SSInterpretFieldInfo(Class klass)
        {
            this.origin = SSMemberOrigin.InterpretScript;
            this.declaringClass = klass;
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
        public override void SetValue(HybInstance _this, HybInstance value)
        {
            if (isStatic)
            {
                declaringClass.runner.globals
                    .SetStaticField(declaringClass, id, value);
                return;
            }

            // it's fast enough to do like this
            if (fieldPtr == -1)
                fieldPtr = _this.fields.GetPtr(id);
            _this.fields.SetByPtr(fieldPtr, value);
        }
    }
}
