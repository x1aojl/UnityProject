// ResPacker.cs
// Created by xiaojl Sep/03/2022
// 资源打包工具

using System.IO;
using UnityEditor;
using UnityEngine;

public partial class ResPacker
{
    // 外部资源目录
    private static string CLIENT_DIR   = PathUtil.GetParentPath(Application.dataPath);
    private static string EXTERNAL_DIR = string.Format("{0}/../../external", Application.dataPath);

    // 需要打进AssetBundle包的根资源的目录
    private static string PREFAB_DIR   = string.Format("{0}/resource/prefab", Application.dataPath);
    private static string MATERIAL_DIR = string.Format("{0}/resource/material", Application.dataPath);
    private static string SHADER_DIR   = string.Format("{0}/resource/shader", Application.dataPath);
    private static string ATLAS_DIR    = string.Format("{0}/resource/atlas", Application.dataPath);
    private static string MICS_DIR     = string.Format("{0}/resource/misc", Application.dataPath);

    // 需要计算md5值的资源目录
    private static string RES_DIR      = string.Format("{0}/resource", Application.dataPath);
    private static string CSHARP_DIR   = string.Format("{0}/Scripts", Application.dataPath);

    // AssetBundle在虚拟文件系统中的目录
    private const string MEDIA_BUNDLE_ROOT = "/medias/bundle";

    // app名称
    private const string APK_NAME = "app.apk";

    // 打包app所需的场景
    private static string[] APP_SCENES = new string[] { "Assets/Scenes/Startup.unity" };

    // 切换平台
    public static void SwitchPlatformImpl(string platform)
    {
        // 通过字符串获取目标平台BuildTarget
        var platformTarget = GetBuildPlatformEnum(platform);

        // 获取打包目标平台组
        var platformGroup = GetBuildPlatformGroup(platformTarget);

        // 切换平台
        if (EditorUserBuildSettings.activeBuildTarget != platformTarget)
            EditorUserBuildSettings.SwitchActiveBuildTarget(platformGroup, platformTarget);
    }

    // 构建完整包
    public static void BuildFullPacket(string output, string version, BuildTarget platform)
    {
        // 检查版本格式
        CheckVersion(version);

        // 获取打包目标平台组
        var platformGroup = GetBuildPlatformGroup(platform);

        // 切换平台
        if (EditorUserBuildSettings.activeBuildTarget != platform)
            EditorUserBuildSettings.SwitchActiveBuildTarget(platformGroup, platform);

        // 获取bin & intermediate目录
        // binDir: ${output}/${platform}/bin/release_${version}
        // intermediateDir: ${output}/${platform}/intermediate/client_${version}
        var binPath = GetReleaseBinPath(output, version);
        var intermediatePath = GetIntermediatePath(output, version);

        // 确保bin & intermediate目录存在
        CheckAndCreateDirectory(binPath);
        CheckAndCreateDirectory(intermediatePath);

        // 保存当前版本的所有关键资源的md5清单文件
        SaveVersionManifest(intermediatePath, platformGroup);

        // 编译资源
        CompileReleaseResources(intermediatePath, version);

        // 生成client_config.json文件
        GenerateReleaseClientConfig(intermediatePath, version);

        // 完整包, 拷贝未压缩资源到StreamingAssets目录下, 所有资源打进安装包
        CopyUncompressReleasePacket(intermediatePath);

        // 准备客户端打包参数
        var appOptions = new BuildPlayerOptions();
        appOptions.target  = platform;
        appOptions.scenes  = APP_SCENES;
        appOptions.options = BuildOptions.None;

        appOptions.locationPathName = binPath + "/" + APK_NAME;

        // 调用unity打包客户端
        BuildPipeline.BuildPlayer(appOptions);

        // version_map输出路径
        var versionOutput = PathUtil.Combine(output, GetBuildPlatformString(platform));

        // 生成version_map.txt
        GenerateReleaseVersionMap(versionOutput, version);
    }

    // 构建补丁包
    public static void BuildPatchPacket(string output, string sourceVersion, string destVersion, BuildTarget platform)
    {
        // 检查版本格式
        CheckVersion(sourceVersion);
        CheckVersion(destVersion);

        // 获取打包目标平台组
        var platformGroup = GetBuildPlatformGroup(platform);

        // 切换平台
        if (EditorUserBuildSettings.activeBuildTarget != platform)
            EditorUserBuildSettings.SwitchActiveBuildTarget(platformGroup, platform);

        // 获取旧版本与新版本的intermediate路径
        var sourceIntermediateDir = GetIntermediatePath(output, sourceVersion);
        var destIntermediateDir = GetIntermediatePath(output, destVersion);

        // 检查旧版本是否存在version.manifest文件，以确保旧版本已经生成version.manifest文件
        Diagnostics.Assert(
            File.Exists(sourceIntermediateDir + "/version.manifest"),
            "Failed to build patch packet from {0} to {1}, source version.manifest file not found.", sourceVersion, destVersion
        );

        // 确保新版本的intermediate路径存在
        CheckAndCreateDirectory(destIntermediateDir);

        // 获取补丁包输出的bin路径
        var binPath = GetPatchBinPath(output, sourceVersion, destVersion);

        // 确保bin路径存在
        CheckAndCreateDirectory(binPath);

        // 保存当前版本(目标版本)的所有关键资源的md5清单文件
        SaveVersionManifest(destIntermediateDir, platformGroup);

        // 检查当前补丁版本csharp是否发生改变 
        Diagnostics.Assert(
            CheckCsharpChanged(sourceIntermediateDir, destIntermediateDir),
            "Failed to build patch packet from {0} to {1}, csharp has changed.", sourceVersion, destVersion
        );

        // 编译当前版本的资源
        CompilePatchResources(sourceVersion, destVersion, sourceIntermediateDir, destIntermediateDir);
    
        GenerateAndCopyPatchClientConfig(sourceIntermediateDir, destIntermediateDir, destVersion);

        CompressAndCopyPatchPacket(destIntermediateDir, binPath, sourceVersion, destVersion);

        // version_map输出路径
        var versionOutput = PathUtil.Combine(output, GetBuildPlatformString(platform));

        // 生成version_map.txt
        GeneratePatchVersionMapManifest(versionOutput, sourceVersion, destVersion, destIntermediateDir);
    }
}
