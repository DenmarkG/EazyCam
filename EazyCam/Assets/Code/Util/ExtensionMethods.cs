using UnityEngine;
using System.Collections;

public static class ExtensionMethods
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }

        return component;
    }

    public static Vector3 GetMidpointTo(this Vector3 vect, Vector3 other)
    {
        // ((B - A / 2) + A
        Vector3 result = other - vect;
        result /= 2;
        result += vect;
        return result;
    }
}
