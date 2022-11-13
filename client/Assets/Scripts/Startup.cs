using System;
using UnityEngine;

class Startup : MonoBehaviour
{
    private void Awake()
    {
        // 初始化日志模块
        Diagnostics.Init();

        try
        {
            OnAwakeImpl();
        }
        catch (Exception e)
        {
            Diagnostics.LogError("Exception caught when startup application.\n    exception:{0}\n    stack:{1}", e.ToString(), e.StackTrace);
        
            throw e;
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            OnDestroyImpl();
        }
        finally
        {
            // 关闭日志模块
            Diagnostics.Shutdown();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        
    }

    private void Update()
    {
        ResourceManager.Update();

        // 累加gc时间
        _gcAccTime += Time.unscaledDeltaTime;

        if (_gcAccTime >= GC_INTERVAL)
        {
            GC.Collect();
            _gcAccTime = 0.0f;
        }    
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        
    }

    private void OnAwakeImpl()
    {
        ResourceManager.Init();
    }

    // 在此函数中处理析构相关逻辑
    private void OnDestroyImpl()
    {
        ResourceManager.Shutdown();

        // 等待gc完成
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    // 每10s做一次gc
    private float _gcAccTime = 0.0f;
    private const float GC_INTERVAL = 10.0f;
}