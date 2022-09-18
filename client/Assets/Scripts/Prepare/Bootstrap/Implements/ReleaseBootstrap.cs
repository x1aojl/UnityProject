using System.Collections;

public class ReleaseBootstrap : Bootstrap
{
    public override IEnumerator Run()
    {
        // 加载更新ui
        UIPrepare.Create();

        // 初始化本地环境
        yield return InitEnv();

        // 安装
        yield return Install();

        // 结束更新
        OnComplete();
    }

    protected override IEnumerator Install()
    {
        // 已经安装过
        if (! _needInstall)
            yield break;

        SetProgressBar("检测本地运行环境,本过程不消耗流量", 0f);

        // 清理根目录
        yield return DeleteAndCreateDirectory(_rootDir);

        // 重新创建patch目录
        FileUtil.CheckAndCreateDirectory(_patchDir);

        // 确保磁盘足够500M
        if (! CheckDiskFreeSpace(INSTALL_DISK_SPACE))
            yield return ShowMessageBox("磁盘不足500M, 请确保有足够的磁盘空间解压安装包", Quit);

        // 安装包内资源copy到patch目录下
        yield return ExportFromReleasePkg();
    }
}