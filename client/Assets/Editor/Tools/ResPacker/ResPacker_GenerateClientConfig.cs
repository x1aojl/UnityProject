// 客户端本地配置文件
// client_config.json eg:
// {
//     "version" : "1.0.0",
//     "release_version" : "1.0.0",
//     "server_config_url" : "http://localhost/server_config.json"
// }

using System.IO;
using System.Text;
using UnityEngine;

public partial class ResPacker
{
    private const string APP_CONFIG = "app.json";

    private static void GenerateReleaseClientConfig(string intermediatePath, string version)
    {
        // 外部client_config文件路径
        var path  = PathUtil.Combine(EXTERNAL_DIR, APP_CONFIG);
        var bytes = File.ReadAllBytes(path);
        var dbase = JsonUtility.FromJson<Dbase>(Encoding.UTF8.GetString(bytes));

        // 保存路径: ${output}/${platform}/intermediate/client_${version}/assets/app.json
        var assetsPath = PathUtil.Combine(intermediatePath, "assets");
        var savePath   = PathUtil.Combine(assetsPath, APP_CONFIG);

        // 设置当前版本号和release版本号
        dbase.Set("version", version);
        dbase.Set("release_version", version);

        File.WriteAllText(savePath, JsonUtility.ToJson(dbase));
    }

    // 生成client_config.json文件
    private static void GeneratePatchClientConfig(string sourceIntermediatePath, string destIntermediatePath, string destVersion)
    {
        var sourceDb = RestoreClientConfig(sourceIntermediatePath);
        var assetsPath = PathUtil.Combine(destIntermediatePath, "assets");

        // 修改最新的版本号
        sourceDb.Set("version", destVersion);

        // 保存路径: ${output}/${platform}/intermediate/client_${version}/assets/app.json
        File.WriteAllText(assetsPath + "/" + APP_CONFIG, JsonUtility.ToJson(sourceDb));
    }

    // 生成client_config.json并且拷贝到patch目录
    private static void GenerateAndCopyPatchClientConfig(string sourceIntermediatePath, string destIntermediatePath, string destVersion)
    {
        GeneratePatchClientConfig(sourceIntermediatePath, destIntermediatePath, destVersion);

        var assetsPath = PathUtil.Combine(destIntermediatePath, "assets");
        var savePath = assetsPath + "/" + APP_CONFIG;

        // 清单文件copy到patch目录下
        var patchPath = PathUtil.Combine(destIntermediatePath, "patch");
        PathUtil.CopyFile(savePath, patchPath + "/" + APP_CONFIG);
    }

    private static Dbase RestoreClientConfig(string intermediatePath)
    {
        // 路径: ${output}/${platform}/intermediate/client_${version}/assets/app.json
        var assetsPath = PathUtil.Combine(intermediatePath, "assets");
        var path       = PathUtil.Combine(assetsPath, APP_CONFIG);

        // 以json文本方式读回来
        var content = File.ReadAllText(path);

        // 反序列化json文本
        return JsonUtility.FromJson<Dbase>(content);
    }
}