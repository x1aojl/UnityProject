using System.Collections;
using UnityEngine;

public class Prepare : MonoBehaviour
{
    private void Awake()
    {
        // 初始化 Prepare UI 管理器
        UIPrepareManager.Init();

        // 启动更新
        StartCoroutine(Run());
    }

    private void OnDestroy()
    {
        // Do nothing
    }

    // 开始游戏
    private void Startup()
    {
        // 析构 Prepare UI 管理器
        UIPrepareManager.Shutdown();

        // 挂载启动脚本
        gameObject.AddComponent<Startup>();
    }

    private IEnumerator Run()
    {
        // 编辑模式下不做更新检查
#if UNITY_EDITOR
        // 启动游戏
        Startup();

        yield return null;
#else
        // 根据平台创建启动boot脚本
        var bootstrap = Bootstrap.CreateInstance();

        yield return StartCoroutine(bootstrap.Run());

        // 更新完毕, 启动游戏
        Startup();
#endif
    }
}