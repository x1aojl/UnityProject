using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public partial class ResPacker
{
    private static void CheckVersion(string version)
    {
        int number;

        // 打包需要指定客户端版本
        if (string.IsNullOrEmpty(version))
            Diagnostics.RaiseException("Build packet need specified client version");

        // 解析版本
        var components = version.Split('.');

        // 版本格式为 'n.n.n'
        if (components.Length != 3 ||
            Int32.TryParse(components[0], out number) == false ||
            Int32.TryParse(components[1], out number) == false ||
            Int32.TryParse(components[2], out number) == false)
        {
            Diagnostics.RaiseException("Invalid version({0}) format.", version);
        }
    }

    private static string GetReleaseBinPath(string rootPath, string version)
    {
        // 规范根路径
        var output = PathUtil.Normalize(rootPath);

        // 获取当前打包的目标平台
        var platform = EditorUserBuildSettings.activeBuildTarget;

        // 生成release包的二进制文件输出路径
        output = PathUtil.Combine(rootPath, GetBuildPlatformString(platform));
        output = PathUtil.Combine(output, "bin");
        output = PathUtil.Combine(output, "release_" + version.Replace('.', '_'));

        // 返回路径
        return output;
    }

    private static string GetPatchBinPath(string rootPath, string sourceVersion, string destVersion)
    {
        // 规范根路径
        var output = PathUtil.Normalize(rootPath);

        // 获取当前打包的目标平台
        var platform = EditorUserBuildSettings.activeBuildTarget;

        // 生成patch包的二进制文件输出路径
        output = PathUtil.Combine(rootPath, GetBuildPlatformString(platform));
        output = PathUtil.Combine(output, "bin");
        output = PathUtil.Combine(output, "patch_" + sourceVersion.Replace(',', '_') + "_to_" + destVersion.Replace(',', '_'));

        // 返回路径
        return output;
    }

    private static string GetIntermediatePath(string rootPath, string version)
    {
        // 规范根路径
        var output = PathUtil.Normalize(rootPath);

        // 获取当前打包的目标平台
        var platform = EditorUserBuildSettings.activeBuildTarget;

        // 获取release包的中间文件输出路径
        output = PathUtil.Combine(output, GetBuildPlatformString(platform));
        output = PathUtil.Combine(output, "intermediate");
        output = PathUtil.Combine(output, "client_" + version.Replace('.', '_'));
        
        // 返回路径
        return output;
    }

    private static string ComputeMd5(string path)
    {
        // 读取文件数据
        var bytes = File.ReadAllBytes(path);

        // 计算md5值
        var md5 = Md5.Encode(bytes);

        // 转换成可读性更好的字符串
        return BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
    }

    private static bool IsMetaFile(string path)
    {
        return path.EndsWith(".meta");
    }

    private static void ExcludeMetaFiles(List<string> files)
    {
        for (var i = files.Count - 1; i >= 0; --i)
        {
            if (IsMetaFile(files[i]))
                files.RemoveAt(i);
        }
    }

    private static string[] FindCsharpScripts()
    {
        // 所有的csharp文件
        var files = new List<string>();

        // 获取csharp根目录下的所有文件
        files.AddRange(PathUtil.GetFiles(CSHARP_DIR, "*.cs", SearchOption.AllDirectories));

        // 移除meta文件
        ExcludeMetaFiles(files);

        // 以数组的方式返回
        return files.ToArray();
    }

    private static string[] FindUnityAllAssets()
    {
        // 存储所有的unity资源
        var assets = new List<string>();

        // 获取所有的unity资源
        assets.AddRange(PathUtil.GetFiles(RES_DIR, "*.*", SearchOption.AllDirectories));

        // 过滤meta文件
        ExcludeMetaFiles(assets);

        // 以数组的方式返回
        return assets.ToArray();
    }

    private static string[] FindUnityRootAssets()
    {
        // 存储所有的unity根资源
        var assets = new List<string>();

        assets.AddRange(PathUtil.GetFiles(PREFAB_DIR,   "*.prefab", SearchOption.AllDirectories));
        assets.AddRange(PathUtil.GetFiles(MATERIAL_DIR, "*.*",      SearchOption.AllDirectories));
        assets.AddRange(PathUtil.GetFiles(SHADER_DIR,   "*.*",      SearchOption.AllDirectories));
        assets.AddRange(PathUtil.GetFiles(ATLAS_DIR,    "*.*",      SearchOption.AllDirectories));
        assets.AddRange(PathUtil.GetFiles(MICS_DIR,     "*.*",      SearchOption.AllDirectories));

        // 过滤meta文件
        ExcludeMetaFiles(assets);

        // 转换为相对路径
        for (var i = 0; i < assets.Count; ++i)
            assets[i] = PathUtil.GetRelativePath(CLIENT_DIR, assets[i]);

        // 以数组的方式返回
        return assets.ToArray();
    }

    private static string[] FindUnityRootAssetsAndDependencies()
    {
        // 获取root资源
        var rootAssets = FindUnityRootAssets();

        // 所有打包资源列表
        var assets = new List<string>();

        foreach (var asset in rootAssets)
        {
            // root资源加入列表
            if (! assets.Contains(asset))
                assets.Add(asset);

            // 获取root资源的依赖资源
            var dependAssets = AssetDatabase.GetDependencies(asset);

            foreach (var dependAsset in dependAssets)
            {
                // 跳过非资源目录的依赖
                if (dependAsset.StartsWith("Assets/resource") == false)
                    continue;

                // 依赖资源加入列表
                if (! assets.Contains(dependAsset))
                    assets.Add(dependAsset);
            }
        }

        // 字典序排序，方便阅读
        assets.Sort();

        return assets.ToArray();
    }

    private static void CheckAndCreateDirectory(string path, bool overwrite = false)
    {
        // 目录已存在
        if (Directory.Exists(path))
        {
            // 不需要覆盖目录则直接返回
            if (overwrite == false)
                return;

            // 需要覆盖目录则先删除目录
            Directory.Delete(path);
        }

        // 创建目录
        PathUtil.CreateDirectoryRecursively(path);
    }

    private static BuildTargetGroup GetBuildPlatformGroup(BuildTarget platform)
    {
        switch (platform)
        {
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return BuildTargetGroup.Standalone;
            default:
                throw new Exception(string.Format("Build target({0}) was not support.", platform));
        }
    }

    public static BuildTarget GetBuildPlatformEnum(string platform)
    {
        switch (platform)
        {
            case "android":
                return BuildTarget.Android;
            case "ios":
                return BuildTarget.iOS;
            case "x86":
                return BuildTarget.StandaloneWindows;
            case "x86_64":
                return BuildTarget.StandaloneWindows64;
            default:
                throw new Exception(String.Format("Build target({0}) was not support", platform));
        }
    }

    public static string GetBuildPlatformString(BuildTarget platform)
    {
        switch (platform)
        {
            case BuildTarget.Android:
                return "aniroid";
            case BuildTarget.iOS:
                return "ios";
            case BuildTarget.StandaloneWindows:
                return "x86";
            case BuildTarget.StandaloneWindows64:
                return "x86_64";
            default:
                throw new Exception(String.Format("Build target({0}) was not support", platform));
        }
    }
}
