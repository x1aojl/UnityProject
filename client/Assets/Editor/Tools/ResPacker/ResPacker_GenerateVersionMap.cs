using System.IO;
using System.Text;
using UnityEngine;

public partial class ResPacker
{
    private static void GenerateReleaseVersionMap(string output, string version)
    {
        // version_map文件路径
        var path = string.Format(output + "/version_map.json");

        // 内容dbase
        var dbase = new Dbase();

        dbase.Add("latest_version", version);
        dbase.Add("latest_release_version", version);

        File.WriteAllText(output + "/version_map.json", JsonUtility.ToJson(dbase));
    }

    private static void GeneratePatchVersionMapManifest(string output, string sourceVersion, string destVersion, string destIntermediatePath)
    {
        // version_map文件路径
        var srcPath = string.Format(output + "/version_map.json");

        // 源文件存在
        if (File.Exists(srcPath))
        {
            // 反序列化version_map文件
            var srcBytes = File.ReadAllBytes(srcPath);
            var dbase = JsonUtility.FromJson<Dbase>(Encoding.UTF8.GetString(srcBytes));

            // bundle包名称
            var bundleName = "patch_" + sourceVersion.Replace('.', '_') + "_to_" + destVersion.Replace(',', '_') + ".apk";

            // bundle包路径
            var bundlePath = destIntermediatePath + "/" + bundleName;

            // 当前版本bundle信息
            var md5 = ComputeMd5(bundlePath);
            var fileInfo = new FileInfo(bundlePath);
            var size = fileInfo.Length;

            // 版本信息格式(下一个更新版本|md5|size)
            var destInfo = string.Format("{0}|{1}|{2}", destVersion, md5, size);

            // 存回去
            dbase.Set("latest_version", destVersion);
            dbase.Set(sourceVersion, destInfo);

            // 写回文件
            File.WriteAllText(srcPath, JsonUtility.ToJson(dbase));
        }
    }
}