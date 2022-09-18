using System;
using System.Collections;
using UnityEngine;

public partial class Bootstrap
{
    // 显示确认框
    protected IEnumerator ShowMessageBox(string content, Action callback)
    {
        var confirmed = false;

        UIPrepare.ShowMessageBox(content, () => {
            confirmed = true;
            callback?.Invoke();
        });

        while (confirmed == false)
            yield return null;

        yield return null;
    }

    // 显示异常提示框
    protected IEnumerator ShowErrorBox(string logMessage, string content)
    {
        UIPrepare.ShowMessageBox(content, () => {
            Quit();
        });

        while (true)
            yield return null;
    }

    // 设置提示
    protected void SetMessageTip(string content)
    {
        UIPrepare.SetMessageTip(content);
    }

    // 设置进度条
    protected void SetProgressBar(string content, float percent)
    {
        UIPrepare.SetProgress(content, percent);
    }

    // 设置进度条
    protected void SetProgressBar(float percent)
    {
        UIPrepare.SetProgress(percent);
    }

    // 显示进度条
    protected void ShowProgressBar()
    {
        UIPrepare.ShowProgress();
    }

    // 隐藏进度条
    protected void HideProgressBar()
    {
        UIPrepare.HideProgress();
    }

    // 关闭app
    protected void Quit()
    {
        Application.Quit();
    }

    // 前往引用商店并关闭app
    protected void JumpToAppStoreAndQuit()
    {
        // 取得应用商店地址，跳转至应用商店
        var url = "";
        if (string.IsNullOrEmpty(url) == false)
            Application.OpenURL(url);

        // 关闭应用
        Application.Quit();
    }
}