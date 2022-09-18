using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Bootstrap
{
    public static Bootstrap CreateInstance()
    {
        // 正常更新流程步骤
        return new ReleaseBootstrap();
    }

    public abstract IEnumerator Run();

    protected abstract IEnumerator Install();

    protected IEnumerator InitEnv()
    {
        // 读写根目录路径
        _rootDir = PathUtil.Combine(Application.persistentDataPath, PRODUCT_NAME);

        // 解压目录路径
        _patchDir = PathUtil.Combine(Application.persistentDataPath, PRODUCT_NAME + "/assets");

        // 确保解压路径存在
        FileUtil.CheckAndCreateDirectory(_patchDir);

        // 创建补丁包
        _patchPkg = new FSPackage(_patchDir);

        // 映射release安装包
#if UNITY_ANDROID && !UNITY_EDITOR
        // 安卓平台StreamingAssets目录为zip目录/assets下, 打包资源父节点也叫做assets, 所以相对zip目录的资源路径为assets/assets
        _releasePkg = new AndroidPackage(Application.dataPath, "assets/assets");
#else
        // release包资源路径
        var releasePath = PathUtil.Combine(Application.streamingAssetsPath, "assets");
        _releasePkg = new FSPackage(releasePath);
#endif

        _needInstall = true;

        yield break;
    }

    protected void OnComplete()
    {
        // 释放release包
        _releasePkg.Close();
        _releasePkg = null;

        // 关闭更新ui
        UIPrepare.Destroy();
    }

    // 读写根目录
    protected string _rootDir;
    // 解压目录
    protected string _patchDir;
    // 首次启动，需要解压安装
    protected bool _needInstall;
    // 安装包(StreamingAssets/assets目录, ios/win下文件目录, android下zip目录)
    protected IPackage _releasePkg;
    // 补丁包(PersistentDataPath/OverRealm/LocalPatch文件目录)
    protected IPackage _patchPkg;
    // release解压安装预留的磁盘空间: 500M
    protected const long INSTALL_DISK_SPACE = 500 * 1024 * 1024L;
    // 项目名称
    private const string PRODUCT_NAME = "local";
}