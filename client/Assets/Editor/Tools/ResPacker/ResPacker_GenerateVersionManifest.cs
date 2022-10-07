using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class ResPacker
{
    private static Dbase SaveVersionManifest(string output, BuildTargetGroup group)
    {
        // 使用dbase保存manifest信息
        var dbase = new Dbase();

        // 分类扫描提取资源信息
        ScanningCsharpScripts(dbase);
        ScanningMacro(dbase, group);
        ScanningUnityResources(dbase);

        // 确保最终目录存在, 且如果已存在需要覆盖
        CheckAndCreateDirectory(output);

        // 路径: ${output}/${platform}/intermediate/client_${version}/version.manifest
        var path = PathUtil.Combine(output, "version.manifest");

        // 写入文件
        File.WriteAllText(path, JsonUtility.ToJson(dbase));

        // 返回dbase
        return dbase;
    }

    private static Dbase RestoreVersionManifest(string output)
    {
        // 路径: ${output}/${platform}/intermediate/client_${version}/version.manifest
        var path = PathUtil.Combine(output, "version.manifest");

        // 以json文本方式读回来
        var content = File.ReadAllText(path);

        // 反序列化json文本
        return JsonUtility.FromJson<Dbase>(content);
    }

    private static void ScanningCsharpScripts(Dbase dbase)
    {
        // 获取所有csharp文件
        var files = FindCsharpScripts();

        // 所有csharp文件的md5保存在一个dbase中
        var db = new Dbase();

        // 遍历所有csharp文件
        foreach (var file in files)
        {
            // 计算文件的md5
            var md5 = ComputeMd5(file);

            // 获取文件的相对路径
            var path = PathUtil.GetRelativePath(Application.dataPath, file);

            // 存入menifest文件
            db.Add(path, md5);
        }

        // 存入上级dbase
        dbase.Add("csharp", db);
    }

    // 当前平台宏定义设置
    private static void ScanningMacro(Dbase dbase, BuildTargetGroup group)
    {
        var macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        // 存入上级dbase
        dbase.Add("macro", macro);
    }

    private static void ScanningUnityResources(Dbase dbase)
    {
        // 获取所有unity资源文件
        var assets = FindUnityAllAssets();

        // 所有unity资源文件的md5保存在一个dbase中
        var db = new Dbase();

        // 遍历所有unity资源文件(包含meta文件)
        foreach (var asset in assets)
        {
            // 计算资源文件的md5
            var md5 = ComputeMd5(asset);

            // 计算资源对应的meta文件的md5
            var metaMd5 = ComputeMd5(asset + ".meta");

            // 获取文件的相对路径
            var path = PathUtil.GetRelativePath(CLIENT_DIR, asset);

            // 存入menifest文件
            db.Add(path, md5 + "|" + metaMd5);
        }

        // 存入上级dbase
        dbase.Add("resource", db);
    }

    // 版本之间c#代码是否发生改变, 检测c#文件md5和宏定义
    private static bool CheckCsharpChanged(string sourceIntermediatePath, string destIntermediatePath)
    {
        // 获取新旧版本的version.manifest信息
        var sourceManifestInfo = RestoreVersionManifest(sourceIntermediatePath);
        var destManifestInfo = RestoreVersionManifest(destIntermediatePath);

        // 比较c#文件md5
        var fileChanged = CheckFilesChanged(sourceManifestInfo, destManifestInfo, "csharp");

        // 比较宏定义
        var sourceMacro  = sourceManifestInfo.Get<string>("macro");
        var destMacro    = sourceManifestInfo.Get<string>("macro");
        var macroChanged = ! sourceMacro.Equals(destMacro);

        return fileChanged || macroChanged;
    }

    // 检测文件是否发生改变
    private static bool CheckFilesChanged(Dbase sourceManifestInfo, Dbase destManifestInfo, string key)
    {
        // 获取两次版本的文件信息
        var sourceFiles = sourceManifestInfo.Get<Dictionary<string, object>>(key);
        var destFiles = destManifestInfo.Get<Dictionary<string, object>>(key);

        // 文件数量发生改变
        if (destFiles.Count != sourceFiles.Count)
            return true;

        // 比对文件md5
        foreach (var pair in sourceFiles)
        {
            // 文件不存在
            if (! destFiles.ContainsKey(pair.Key))
                return true;

            var sourceMd5 = pair.Value;
            var destMd5 = destFiles[pair.Key];

            // md5发生变化
            if (! sourceMd5.Equals(destMd5))
                return true;
        }

        return false;
    }
}