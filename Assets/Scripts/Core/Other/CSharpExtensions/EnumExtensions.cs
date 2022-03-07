using System;
using System.Collections;
using UnityEngine;

namespace Core.Other.CSharpExtensions
{
    public static class EnumExtensions
    {
        public static T[] GetEnumValues<T>() where T : struct
        {
            IList enumValues;
            try
            {
                enumValues = Enum.GetValues(typeof(T));
            }
            catch (ArgumentException e)
            {
                Debug.LogError("It's not enum " + typeof(T));
                return new T[0];
            }
            T[] values = new T[enumValues.Count];
            for (int i = 0; i < enumValues.Count; i++)
            {
                values[i] = (T)enumValues[i];
            }
            return values;
        }

        public static string[] GetEnumToStringValues<T>() where T : struct
        {
            T[] enumValues = GetEnumValues<T>();
            string[] enumStringValues = new string[enumValues.Length];
            for (int i = 0; i < enumStringValues.Length; i++)
            {
                enumStringValues[i] = enumValues[i].ToString();
            }
            return enumStringValues;
        }
    }
}
