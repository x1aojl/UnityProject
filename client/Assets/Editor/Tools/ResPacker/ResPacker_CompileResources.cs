using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        // 生成客户端资源清单文件
        GenerateReleaseResourceManifest(output, version, bundles);
    }

    private static void CompilePatchResources(string sourceVersion, string destVersion, string sourceIntermediatePath, string destIntermediatePath)
    {
        // 处理目标版本的资源
        // ${output}/${platform}/intermediate/client_${version}/assets
        var assetRootPath = PathUtil.Combine(destIntermediatePath, "assets");

        // 获取assets下的子目录
        var bundleRootPath = PathUtil.Combine(assetRootPath, "bundle");

        // 源目标assets文件夹
        var sourcePath = PathUtil.Combine(sourceIntermediatePath, "assets");
        var destPath = PathUtil.Combine(destIntermediatePath, "assets");

        // 创建patch文件夹
        var patchPath = PathUtil.Combine(destIntermediatePath, "patch");

        CheckAndCreateDirectory(patchPath);

        // 获取新旧版本的version.manifest信息
        var sourceManifestInfo = RestoreVersionManifest(sourceIntermediatePath);
        var destManifestInfo = RestoreVersionManifest(destIntermediatePath);

        // 打AssetBundle差异包
        var sourceManifestPath = PathUtil.Combine(sourcePath, "client.manifest");
        var content = File.ReadAllText(sourceManifestPath);
        var sourceManifest = JsonUtility.FromJson<Dbase>(content);

        // 补丁包名
        var patchBundleName = "patch_" + sourceVersion.Replace(',', '_') + "_to_" + destVersion.Replace(',', '_');

        // 打差异包(先放到临时目录中)
        var bundles = BuildPatchAssetBundles(
            sourceManifestInfo.Get<Dictionary<string, object>>("resource"),
            destManifestInfo.Get<Dictionary<string, object>>("resource"),
            sourceManifest.Get<Dictionary<string, object>>("assets"),
            bundleRootPath,
            patchBundleName
        );

        // 有打包，将补丁包从assets.bundle目录拷贝到patch/bundle中
        if (bundles.Length > 0)
            PathUtil.CopyFiles(bundleRootPath + "/" + patchBundleName, patchPath + "bundle" + patchBundleName);

        // 生成客户端资源清单文件
        GeneratePatchResourceManifest(destIntermediatePath, destVersion, sourceManifest, bundles);

        // 清单文件copy到patch目录下
        PathUtil.CopyFile(destPath + "/client.manifest", patchPath + "/client.manifest");
    }
}
