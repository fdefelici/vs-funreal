using System;

namespace FUnreal
{
    public static class XObjectValidator
    {
        public static FUnrealGenericResult Validate(object instance)
        {
            FUnrealGenericResult result = FUnrealGenericResult.Success();
            
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
                        //string msg = $"Validator '{attrValidator.GetType().Name}' fails on '{instanceType.Name}.{field.Name}' with value '{value}'";
                        string msg = $"Validator '{attrValidator.Description()}' fails on '{instanceType.Name}.{field.Name}' with value '{value}'";
                        result += FUnrealGenericResult.Failure(msg);
                    }
                }

                { 
                    //Recursion on field avoiding string or primitive to avoid stackoverflow)
                    if (field.FieldType == typeof(string)) continue;
                    if (field.FieldType.IsPrimitive) continue;
                
                    var value = field.GetValue(instance);
                    if (value == null) break;

                    if (field.FieldType.IsArray)
                    {
                        var valueAsArray = value as Array;
                        result += ValidateArray(valueAsArray);
                    } 
                    else
                    {
                        result += Validate(value);
                    }
                }
            }
            return result;
        }

        private static FUnrealGenericResult ValidateArray(Array values)
        {
            FUnrealGenericResult result = FUnrealGenericResult.Success();

            foreach (var value in values)
            {
                result += Validate(value);
            }
            return result;
        }
    }
}
