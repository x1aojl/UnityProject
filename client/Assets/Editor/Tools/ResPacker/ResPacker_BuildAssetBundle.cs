using System.Collections.Generic;
using UnityEditor;

public partial class ResPacker
{
    private static AssetBundleBuild[] BuildReleaseAssetBundles(string output, string version)
    {
        // 获取bundle名
        var bundleName = "release_" + version.Replace('.', '_');

        // 收集需要打包的根资源列表和依赖资源列表
        var assets = FindUnityRootAssetsAndDependencies();

        // 创建打包参数数组
        var bundles = new AssetBundleBuild[1];

        // 目前采用大包方案，只打一个包
        bundles[0] = new AssetBundleBuild();
        bundles[0].assetBundleName    = bundleName;
        bundles[0].assetBundleVariant = "";
        bundles[0].assetNames         = assets;

        // 确保目录存在
        // 目录: .../${platform}/intermediate/client_${version}/assets/bundles
        CheckAndCreateDirectory(output);

        // 获取当前打包的目标平台
        var platform = EditorUserBuildSettings.activeBuildTarget;

        // 执行打包
        BuildPipeline.BuildAssetBundles(output, bundles, BuildAssetBundleOptions.UncompressedAssetBundle, platform);

        // 返回AssetBundle列表信息
        return bundles;
    }

    private static AssetBundleBuild[] BuildPatchAssetBundles(Dictionary<string, object> sourceAssetMap,
                                                      Dictionary<string, object> destAssetMap,
                                                      Dictionary<string, object> sourceManifest,
                                                      string output, string patchBundleName)
    {
        // 获取当前版本的根资源和相关依赖资源
        var assets = FindUnityRootAssetsAndDependencies();

        // key:   asset bundle名
        // value: asset列表
        var assetBundleMap = new Dictionary<string, List<string>>();

        // 当前补丁资源
        var patchAssets = new List<string>();

        // 遍历资源
        foreach (var asset in assets)
        {
            // 如果旧版本的client.manifest中不存在此资源
            // 获取旧版本资源在新版本中发生了改变(包括依赖资源)
            // 打进patch包中
            if (sourceManifest.ContainsKey(asset) == false || IsResourceChanged(asset, sourceAssetMap, destAssetMap))
            {
                patchAssets.Add(asset);
            }
            // 此资源在旧版本中也以根资源的形式存在, 且未发生改变(包括依赖资源)
            else
            {
                // 获取此资源所在的旧包包名
                var bundleName = (string)sourceManifest[asset];

                // 此旧包需要被引用
                if (assetBundleMap.ContainsKey(bundleName) == false)
                    assetBundleMap.Add(bundleName, new List<string>());

                // 加入资源
                assetBundleMap[bundleName].Add(asset);
            }
        }

        // 如果有资源变化，增加补丁包，没有则按上个版本的bundle结构打包
        if (patchAssets.Count > 0)
            assetBundleMap.Add(patchBundleName, patchAssets);

        // 创建bundle描述结构
        var bundles = new AssetBundleBuild[assetBundleMap.Count];

        // 索引
        var index = 0;

        // 生成打包的参数
        foreach (var kvp in assetBundleMap)
        {
            var bundle = new AssetBundleBuild();
            bundle.assetBundleName    = kvp.Key;
            bundle.assetBundleVariant = "";
            bundle.assetNames         = kvp.Value.ToArray();

            bundles[index++] = bundle;
        }

        // 确保写入目录存在
        CheckAndCreateDirectory(output);

        var platform = EditorUserBuildSettings.activeBuildTarget;

        BuildPipeline.BuildAssetBundles(output, bundles, BuildAssetBundleOptions.UncompressedAssetBundle, platform);

        return bundles;
    }

    private static bool IsResourceChanged(string rootAsset, Dictionary<string, object> sourceAssetMap, Dictionary<string, object> destAssetMap)
    {
        // 如果资源在旧版本中不存在，视为发生变化
        if (sourceAssetMap.ContainsKey(rootAsset) == false)
            return true;

        // 取资源在新旧版本的md5值
        var sourceMd5 = sourceAssetMap[rootAsset];
        var destMd5 = destAssetMap[rootAsset];

        // 如果资源发生了改变，直接放入patch包
        if (sourceMd5.Equals(destMd5) == false)
            return true;

        // 获取资源的依赖资源
        var dependAssets = AssetDatabase.GetDependencies(rootAsset);

        // 遍历比较依赖资源
        foreach (var dependAsset in dependAssets)
        {
            // 跳过非资源目录的依赖
            if (dependAsset.StartsWith("Assets/resource") == false)
                continue;

            // 依赖资源在旧版本中不存在，视为发生改变
            if (sourceAssetMap.ContainsKey(dependAsset) == false)
                return true;

            // 取依赖资源在新旧版本中的md5值
            sourceMd5 = sourceAssetMap[dependAsset];
            destMd5 = destAssetMap[dependAsset];

            // 依赖资源发生改变，根资源视为改变
            if (sourceMd5.Equals(destMd5) == false)
                return true;
        }

        // 资源未发生改变
        return false;
    }
}
