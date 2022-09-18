// Diagnostics.cs
// Created by xiaojl Sep/17/2022
// 日志工具

using System;
using UnityEngine;

public partial class Diagnostics
{
    public enum LogFilterLevel
    {
        // 过滤所有日志(不显示任何日志)
        None      = -1,
        // 与LogType的枚举值一致
        Error     = 0,
        Assert    = 1,
        Warning   = 2,
        Log       = 3,
        Exception = 4,
    }

    public static Application.LogCallback LogMessageReceived;

    public static void Init()
    {
        // 只输出Error日志
        m_LogLevel = LogFilterLevel.Exception;
    }

    public static void Shutdown()
    {
        // 清空日志hook委托列表
        LogMessageReceived = delegate { };
    }

    public static void Log(string content)                             { LogInternal(LogType.Log, content); }
    public static void Log(string format, params object[] args)        { LogInternal(LogType.Log, string.Format(format, args)); }
    public static void LogWarning(string content)                      { LogInternal(LogType.Warning, content); }
    public static void LogWarning(string format, params object[] args) { LogInternal(LogType.Warning, string.Format(format, args)); }
    public static void LogError(string content)                        { LogInternal(LogType.Error, content); }
    public static void LogError(string format, params object[] args)   { LogInternal(LogType.Error, string.Format(format, args)); }

    public static void Assert(bool condition)
    {
        if (! condition)
            RaiseException("Assure failed");
    }

    public static void Assert(bool condition, string format, params object[] args)
    {
        if (! condition)
            RaiseException(format, args);
    }

    public static void RaiseException(string format, params object[] args)
    {
        throw new Exception(string.Format(format, args));
    }

    public static void ReportException(Exception e, string format, params object[] args)
    {
        // Get format extra message
        var message = string.Format(format, args);

        // Get inner exception.
        if (e.InnerException != null)
            e = e.InnerException;

        // Log error
        Diagnostics.LogError(string.Format("{0}\nException={1}\nStack={2}", message, e.Message, e.StackTrace));
    }

    public static void OnLogLevelChanged(LogFilterLevel level)
    {
        m_LogLevel = level;
    }

    private static void LogInternal(LogType level, string content)
    {
        // 检查日志等级
        if ((int)level > (int)m_LogLevel)
            return;

        // 分等级处理日志
        switch (level)
        {
            case LogType.Log:
                Debug.Log(content);
                break;
            case LogType.Warning:
                Debug.LogWarning(content);
                break;
            case LogType.Error:
            default:
                Debug.LogError(content);
                break;
        }

        // 调用日志hook委托函数
        if (LogMessageReceived != null)
            LogMessageReceived(content, null, level);
    }

    private static LogFilterLevel m_LogLevel = LogFilterLevel.Exception;
}