// UIPrepareManager.cs
// Created by xiaojl Sep/18/2022
// 准备阶段 UI 管理

using UnityEngine;

public static class UIPrepareManager
{
    internal static void Init()
    {
        var prefab = Resources.Load<GameObject>(ROOT_PATH);
        _root = Object.Instantiate(prefab).transform;
    }

    internal static void Shutdown()
    {
        UnityObject.Destroy(_root.gameObject);
        _root = null;
    }

    internal static GameObject Open(string path)
    {
        var prefab = Resources.Load<GameObject>(path);
        var go = Object.Instantiate(prefab);
        return go;
    }

    internal static void Close(GameObject go)
    {
        UnityObject.Destroy(go);
    }

    private const string ROOT_PATH = "ui_prepare_root";
    private static Transform _root;
}