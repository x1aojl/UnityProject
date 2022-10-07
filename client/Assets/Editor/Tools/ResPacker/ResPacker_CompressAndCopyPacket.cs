using UnityEngine;

public partial class ResPacker
{
    // 完整包，拷贝release的所有未压缩资源到StreamingAssets下
    private static void CopyUncompressReleasePacket(string output)
    {
        // 获取资源包根目录
        var assetsRootPath = PathUtil.Combine(output, "assets");

        var destPath = Application.streamingAssetsPath + "/assets";

        // 确保streamingAssets目录存在，如果已经存在先删除
        CheckAndCreateDirectory(destPath, true);

        // 将所有文件直接拷贝到Assets/StreamingAssets下
        PathUtil.CopyFiles(assetsRootPath, destPath);
    }

    private static void CompressAndCopyPatchPacket(string destIntermediatePath, string binPath, string sourceVersion, string destVersion)
    {
        var patchRootPath = destIntermediatePath + "/patch";

        var bundleName = "patch_" + sourceVersion.Replace(',', '_') + "_to_" + destVersion.Replace(',', '_');

        // 压缩后的资源包路径
        var bundlePath = PathUtil.Combine(destIntermediatePath, "patch");

        // 未压缩的zip文件的路径
        var uncompressedPath = destIntermediatePath + "/" + bundleName + ".zip";

        // 打包到zip文件
        // 输出路径:
        ZipBundle.Pack(patchRootPath, uncompressedPath);

        // 将.zip文件通过Lzma压缩成.apk文件
        var compressPath = destIntermediatePath + "/" + bundleName + ".apk";
        Lzma.Encode(uncompressedPath, compressPath);

        // 将压缩包拷贝到bin下
        PathUtil.CopyFiles(compressPath, binPath + "/" + bundleName + ".apk");
    }
}
