using System;
using System.Reflection;

public static class ReflectionUtility
{
    public static bool HasField(object obj, string fieldName, out FieldInfo fieldInfo, bool includeNonPublic = false)
    {
        Type type = obj.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        if (includeNonPublic)
        {
            flags |= BindingFlags.NonPublic;
        }
        fieldInfo = type.GetField(fieldName, flags);
        return fieldInfo != null;
    }

    public static void SetFieldValue(object obj, string fieldName, object value, bool includeNonPublic = false)
    {
        if (HasField(obj, fieldName, out var fieldInfo, includeNonPublic))
        {
            fieldInfo.SetValue(obj, value);
        }
    }

    public static bool HasMethod(object obj, string methodName, out MethodInfo methodInfo, bool includeNonPublic = false)
    {
        Type type = obj.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        if (includeNonPublic)
        {
            flags |= BindingFlags.NonPublic;
        }
        methodInfo = type.GetMethod(methodName, flags);
        return methodInfo != null;
    }

    public static T BindToDelegate<T>(object obj, string methodName, bool includeNonPublic = false) where T : Delegate
    {
        if (!HasMethod(obj, methodName, out var methodInfo, includeNonPublic)) return null;

        return (T)Delegate.CreateDelegate(typeof(T), obj, methodInfo);
    }
}