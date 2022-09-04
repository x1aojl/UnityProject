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
}
