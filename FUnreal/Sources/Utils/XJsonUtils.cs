using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal
{
    public static class XJsonUtils
    {
        public static bool TryFromJsonString<T>(string jsonStr, out T instance) where T : new()
        {
            return TryFromJsonString<T>(jsonStr, false, out instance);
        }

        /// <summary>
        /// In Permissive mode, parser will match field name ignoring case, but also allow to parse everything (including objects or array) as string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <param name="permissive"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool TryFromJsonString<T>(string jsonStr, bool permissive, out T instance) where T : new()
        {
            instance = default;
            if (string.IsNullOrEmpty(jsonStr)) return false;
            if (string.IsNullOrEmpty(jsonStr.Trim())) return false;
            //exception
            JObject jsonObj = JObject.Parse(jsonStr);

            object objInstance = new T();
            bool result = TryFromJsonObject(jsonObj, permissive, ref objInstance);
            if (!result) return false;
            instance = (T)objInstance;
            return true;
        }

        private static bool TryFromJsonObject(JObject jsonObj, bool permissive, ref object instance)
        {
            Type instanceType = instance.GetType();

            //Just to do it once, for the whole object and only if needed.
            Dictionary<string, object> allJsonFieldsCache = null;

            foreach (var eachField in instanceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = null;
                JToken jsonField = null;
                if (jsonObj.ContainsKey(eachField.Name))
                {
                    fieldName = eachField.Name;
                    jsonField = jsonObj[fieldName];
                }
                else if (permissive)
                {
                    if (allJsonFieldsCache == null) allJsonFieldsCache = jsonObj.ToObject<Dictionary<string, object>>(); //assign only once
                    foreach (var jsonFieldNameValue in allJsonFieldsCache)
                    {
                        if (jsonFieldNameValue.Key.Equals(eachField.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            fieldName = jsonFieldNameValue.Key;
                            jsonField = jsonObj[fieldName];
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(fieldName)) continue;


                bool success = TryGetValue(eachField.FieldType, permissive, jsonField, out object fieldValue);
                if (success)
                {
                    eachField.SetValue(instance, fieldValue);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryFromJsonArray(JArray jsonArray, bool permissive, ref Array instance)
        {
            Type elementType = instance.GetType().GetElementType();

            for (int index = 0; index < jsonArray.Count; ++index)
            {
                var jsonItem = jsonArray[index];

                bool valueParsed = TryGetValue(elementType, permissive, jsonItem, out object value);
                if (valueParsed)
                {
                    instance.SetValue(value, index);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to convert a json value to the expected csharpType
        /// NOTE: Permessive allow to convert any json value (including object or array) to string if requested
        /// </summary>
        /// <param name="expectedCSharpType"></param>
        /// <param name="jsonToken"></param>
        /// <param name="csharpValue"></param>
        /// <returns></returns>
        private static bool TryGetValue(Type expectedCSharpType, bool permissive, JToken jsonToken, out object csharpValue)
        {
            if (expectedCSharpType == typeof(string))
            {
                if (jsonToken.Type == JTokenType.String)
                {
                    csharpValue = (string)jsonToken;
                    return true;
                }
                //else if (permissive && jsonToken.Type != JTokenType.Object && jsonToken.Type != JTokenType.Array)
                else if (permissive)
                {
                    //casting to string like this "(string)jsonToken" works only for primitive type (Not JObject and Not JArray);
                    //Instead JToken.ToString works for all types
                    csharpValue = jsonToken.ToString(Formatting.None);
                    return true;
                }
            }
            else if (expectedCSharpType == typeof(int))
            {
                if (jsonToken.Type == JTokenType.Integer)
                {
                    csharpValue = (int)jsonToken;
                    return true;
                }
            }
            else if (expectedCSharpType == typeof(float))
            {
                if (jsonToken.Type == JTokenType.Float || jsonToken.Type == JTokenType.Integer)
                {
                    csharpValue = (float)jsonToken;
                    return true;
                }
            }
            else if (expectedCSharpType == typeof(bool))
            {
                if (jsonToken.Type == JTokenType.Boolean)
                {
                    csharpValue = (bool)jsonToken;
                    return true;
                }
            }
            else if (expectedCSharpType.IsArray)
            {
                var jsonArray = (JArray)jsonToken;

                int arraySize = jsonArray.Count;
                Array arrayIstance = Array.CreateInstance(expectedCSharpType.GetElementType(), arraySize);
                bool success = TryFromJsonArray(jsonArray, permissive, ref arrayIstance);
                csharpValue = arrayIstance;
                if (success) return true;
            }
            else if (expectedCSharpType.IsValueType || expectedCSharpType.IsClass) //struct or class (array/list are considered class)
            {
                if (jsonToken.Type == JTokenType.Object)
                {
                    var jsonObjChild = (JObject)jsonToken;

                    object childinstance = Activator.CreateInstance(expectedCSharpType);
                    bool success = TryFromJsonObject(jsonObjChild, permissive, ref childinstance);
                    csharpValue = childinstance;
                    if (success) return true;
                }
            }

            csharpValue = null;
            return false;
        }
    }
}
