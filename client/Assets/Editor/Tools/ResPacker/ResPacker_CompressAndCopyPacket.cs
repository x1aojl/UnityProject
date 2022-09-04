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
}
