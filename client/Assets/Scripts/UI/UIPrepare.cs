using System;
using UnityEngine;

// 更新ui
public class UIPrepare : MonoBehaviour
{
    private static UIPrepare _instance;

    #region ui

    private void Awake()
    {
        _instance = this;
    }

    // 弹出提示框
    private void _ShowMessageBox(string desc, Action callback)
    {
        // TODO
    }

    // 设置标题
    private void _SetTitle(string desc)
    {
        // TODO
    }

    // 设置提示
    private void _SetTip(string desc)
    {
        // TODO
    }

    // 设置进度值
    private void _SetProgress(float value)
    {
        // TODO
    }

    // 显示进度条
    private void _ShowProgress()
    {
        // TODO
    }

    // 隐藏进度条
    private void _HideProgress()
    {
        // TODO
    }

    // 销毁
    void _Destroy()
    {
        GameObject.Destroy(gameObject);
    }
    #endregion

    #region static functions

    // 加载
    public static void Create()
    {
        // TODO
    }

    // 销毁更新ui
    public static void Destroy()
    {
        // TODO
    }

    // 弹出提示框
    public static void ShowMessageBox(string desc, Action callback)
    {
        if (_instance != null)
            _instance._ShowMessageBox(desc, callback);
    }

    public static void SetMessageTip(string desc)
    {
        if (_instance != null)
            _instance._SetTip(desc);
    }

    // 设置更新进度条
    public static void SetProgress(string title, float progress)
    {
        if (_instance != null)
        {
            _instance._SetTitle(title);
            _instance._SetProgress(progress);
        }
    }

    // 设置更新进度条
    public static void SetProgress(float progress)
    {
        if (_instance != null)
            _instance._SetProgress(progress);
    }

    // 显示更新进度条
    public static void ShowProgress()
    {
        if (_instance != null)
            _instance._ShowProgress();
    }

    // 隐藏更新进度条
    public static void HideProgress()
    {
        if (_instance != null)
            _instance._HideProgress();
    }

    #endregion
}

