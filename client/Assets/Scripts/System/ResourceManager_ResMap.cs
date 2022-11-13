using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class ResourceManager
{
    private static void LoadAssetMap()
    {
        _assetMap  = new Dictionary<string, string>();
        _bundleMap = new Dictionary<string, AssetBundle>();

        // 解析client.json的磁盘路径
        var path = FileSystem.ResolveDiskPath("/medias/client.manifest");

        // 读取client.json文件
        var content = File.ReadAllText(path);

        // 解析client.json文件
        var dbase = JsonUtility.FromJson<Dbase>(content);

        // 读取资源
        var bundles = dbase.Get<Dictionary<string, object>>("bundles");
        var assets  = dbase.Get<Dictionary<string, object>>("assets");

        // 初始化资源包映射表
        foreach (var kvp in bundles)
        {
            // 解析资源包的磁盘路径
            var bundlePath = FileSystem.ResolveDiskPath((string)kvp.Value);

            // 加载资源包
            var bundle = AssetBundle.LoadFromFile(bundlePath);

            // 加入映射表
            _bundleMap.Add(kvp.Key, bundle);
        }

        // 初始化资源映射表
        foreach (var kvp in assets)
            _assetMap.Add(kvp.Key, (string)kvp.Value);
    }

    private static void UnloadAssetMap()
    {
        foreach (var bundle in _bundleMap.Values)
            bundle.Unload(true);

        _assetMap  = null;
        _bundleMap = null;
    }

    private static AssetBundle ResolveBundle(string path)
    {
        string bundleName;

        // 取资源对应的资源包名
        if (_assetMap.TryGetValue(path, out bundleName) == false)
            return null;

        AssetBundle bundle;

        // 获取资源包对象
        if (_bundleMap.TryGetValue(bundleName, out bundle))
            return bundle;

        // 资源包不存在
        return null;
    }

    private static Dictionary<string, string>      _assetMap;  // AssetName  : BundleName
    private static Dictionary<string, AssetBundle> _bundleMap; // BundleName : AssetBundle
}