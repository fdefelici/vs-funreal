using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public static class XObjectValidator
    {
        //'errorAsWarn' flag to prevent UnitTest from failing due to exception thrown by Error log
        //Eventually externalize logging and return a collection of validation error 
        public static bool Validate(object instance, bool errorAsWarn = false)
        {
            var instanceType = instance.GetType();
            var fields = instanceType.GetFields();
            foreach (var field in fields)
            {
                object[] attrs = field.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (!(attr is AXAttrValidator)) continue;
                    var attrValidator = (AXAttrValidator)attr;

                    var value = field.GetValue(instance);
                    if (!attrValidator.Validate(value))
                    {
                        string msg = $"Validator '{attrValidator.GetType().Name}' fails on '{instanceType.Name}.{field.Name}' with value '{value}'";
                        if (errorAsWarn)
                        {
                            XDebug.Warn(msg);
                        }
                        else
                        {
                            XDebug.Erro(msg);
                        }
                        return false;
                    }
                }

                //Recursion on field that is Serializable  (Note: string and primitives are Serializable too and need to be skipped to avoid stackoverflow)
                if (field.FieldType == typeof(string)) continue;
                if (field.FieldType.IsPrimitive) continue;
                object[] classAttr = field.GetType().GetCustomAttributes(true);
                foreach (var attr in classAttr)
                {
                    if (attr is SerializableAttribute)
                    {
                        var value = field.GetValue(instance);
                        if (value == null) break;
                        if (!Validate(value, errorAsWarn)) return false;
                        break;
                    }
                }
            }
            return true;
        }
    }
}
