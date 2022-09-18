using System;
using System.IO;

public class FileUtil
{
    // 移除只读属性
    public static void RemoveReadonly(string path)
    {
        FileSystemInfo info = null;
        if (! File.Exists(path))
            return;

        info = new FileInfo(path);

        var isReadonly = false;

        try
        {
            isReadonly = (info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            if (isReadonly)
                info.Attributes = FileAttributes.Normal;
        }
        catch (Exception e)
        {
            // TODO
        }
    }

    // 有些安卓手机，直接删除文件会删除失败，有人建议是先重名，然后再删除
    public static void Delete(string path)
    {
        if (! File.Exists(path))
            return;

        try
        {
            File.Delete(path);
        }
        catch
        {
            // TODO
        }

        if (! File.Exists(path))
            return;

        RemoveReadonly(path);

        string tmp = path + "_" + UnityEngine.Random.Range(0, int.MaxValue);
        File.Move(path, tmp);
        File.Delete(tmp);
    }

    // 写文件
    public static void Write(string path, byte[] bytes)
    {
        string tmp = path + ".tmp";
        File.WriteAllBytes(tmp, bytes);

        if (File.Exists(path))
            Delete(path);

        File.Move(tmp, path);

#if UNITY_IPHONE && ! UNITY_EDITOR
        UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif
    }

    public static void CheckAndCreateDirectory(string path, bool overwrite = false)
    {
        // 目录已存在
        if (Directory.Exists(path))
        {
            // 不需要覆盖目录则直接返回
            if (overwrite == false)
                return;

            // 需要覆盖目录则先删除目录
            Directory.Delete(path, true);
        }

        // 创建目录
        PathUtil.CreateDirectoryRecursively(path);
    }

    // 文件重命名
    public static void Rename(string path, string newPath)
    {
        if (File.Exists(newPath))
            Delete(newPath);

        // 移动文件
        File.Move(path, newPath);
    }
}