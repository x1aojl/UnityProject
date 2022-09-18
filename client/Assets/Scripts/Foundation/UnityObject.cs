// UnityObject.cs
// Created by xiaojl Sep/18/2022
// Unity对象类

using UnityEngine;
using Object = UnityEngine.Object;

public static class UnityObject
{
    public static void Destroy(Object ob)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            Object.Destroy(ob);
        else
            Object.DestroyImmediate(ob);
#else
        Object.Destroy(ob);
#endif
    }

    public static void Destroy(Object ob, float delay)
    {
        Object.Destroy(ob, delay);
    }

    public static bool IsDestroyed(Object ob)
    {
        return ob == null;
    }
}