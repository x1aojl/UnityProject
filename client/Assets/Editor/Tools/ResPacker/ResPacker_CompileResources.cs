public partial class ResPacker
{
    private static void CompileReleaseResources(string output, string version)
    {
        // ${output}/${platform}/intermediate/client_${version}/assets
        var assetRootPath = PathUtil.Combine(output, "assets");

        // 获取assets下的子目录
        var bundleRootPath = PathUtil.Combine(assetRootPath, "bundle");

        // 生成AssetBundle资源包
        var bundles = BuildReleaseAssetBundles(bundleRootPath, version);
    }
}
