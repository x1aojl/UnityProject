// ResPacker.cs
// Created by xiaojl Sep/03/2022
// 资源打包工具

using UnityEditor;
using UnityEngine;

public partial class ResPacker
{
    // 外部资源目录
    private static string CLIENT_DIR = PathUtil.GetParentPath(Application.dataPath);

    // 需要打进AssetBundle包的根资源的目录
    private static string PREFAB_DIR   = string.Format("{0}/resource/prefab", Application.dataPath);
    private static string MATERIAL_DIR = string.Format("{0}/resource/material", Application.dataPath);
    private static string SHADER_DIR   = string.Format("{0}/resource/shader", Application.dataPath);
    private static string ATLAS_DIR    = string.Format("{0}/resource/atlas", Application.dataPath);
    private static string MICS_DIR     = string.Format("{0}/resource/misc", Application.dataPath);

    private const string APK_NAME = "app.apk";

    private static string[] APP_SCENES = new string[] { "Assets/Scenes/Startup.unity" };

    // 构建完整包
    public static void BuildFullPacket(string output, string version, BuildTarget platform)
    {
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

        // 确保intermediate目录存在
        CheckAndCreateDirectory(binPath);
        CheckAndCreateDirectory(intermediatePath);

        // 编译资源
        CompileReleaseResources(intermediatePath, version);

        // 完整包，拷贝未压缩资源到StreamingAssets目录下，所有资源打进安装包
        CopyUncompressReleasePacket(intermediatePath);

        // 准备客户端打包参数
        var appOptions = new BuildPlayerOptions();
        appOptions.target  = platform;
        appOptions.scenes  = APP_SCENES;
        appOptions.options = BuildOptions.None;

        appOptions.locationPathName = binPath + "/" + APK_NAME;

        // 调用unity打包客户端
        BuildPipeline.BuildPlayer(appOptions);
    }
}
