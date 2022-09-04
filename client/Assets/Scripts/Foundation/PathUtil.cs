// PathUtil.cs
// Created by xiaojl Sep/04/2022
// 路径处理工具

using System;
using System.Collections.Generic;
using System.IO;

public class PathUtil
{
    public static string Combine(string path0, string path1)
    {
        return string.Format("{0}/{1}", path0, path1);
    }

    public static void CreateDirectoryRecursively(string path)
    {
        var entries = new List<string>();

        // 自顶向下遍历各级未存在的目录
        while (Directory.Exists(path) == false)
        {
            // 记录当前未创建目录
            entries.Add(path);

            // 找到最后一级目录的分割符
            var pos = path.LastIndexOf('/');

            // 已经到顶(每级目录都不存在!)
            if (pos < 0)
                break;

            // 移到上级目录
            path = path.Substring(0, pos);
        }

        // 循环创建目录
        for (int i = entries.Count - 1; i >= 0; --i)
            Directory.CreateDirectory(entries[i]);
    }

    public static string GetParentPath(string path, int level = 1)
    {
        int curPos = path.Length;
        for (var i = 0; i < level; ++i)
        {
            curPos = path.LastIndexOf('/', curPos - 1);
            if (curPos < 0)
                throw new Exception(String.Format("Can't get parent path. path={0}, level={1}", path, level));
        }

        if (curPos == path.Length)
            return path;

        return path.Substring(0, curPos);
    }

    public static string GetRelativePath(string rootPath, string absPath)
    {
        var pos = absPath.IndexOf(rootPath);
        if (pos != 0 || absPath.Length < pos + 1)
            return null;

        return absPath.Substring(rootPath.Length + 1);
    }

    public static string[] GetFiles(string path, string pattern, SearchOption option)
    {
        var entries = Directory.GetFiles(path, pattern, option);
        for (var i = 0; i < entries.Length; i++)
            entries[i] = Normalize(entries[i]);

        return entries;
    }

    public static void CopyFile(string sourceFile, string destFile)
    {
        // 获取目标文件的父路径
        var parentPath = GetParentPath(destFile);

        // 如果目标文件的父路径不存在，创建之
        if (Directory.Exists(parentPath) == false)
            Directory.CreateDirectory(parentPath);

        // 拷贝文件
        File.Copy(sourceFile, destFile);
    }

    public static void CopyFiles(string sourceDir, string destDir,
                                 string patten = "*.*", SearchOption option = SearchOption.AllDirectories)
    {
        // 获取源路径下的所有文件
        var files = GetFiles(sourceDir, patten, option);

        // 拷贝所有文件
        for (var i = 0; i < files.Length; ++i)
        {
            // 获取源文件相对sourceDir的相对路径
            var relativePath = GetRelativePath(sourceDir, files[i]);

            // 获取目标路径
            var path = PathUtil.Combine(destDir, relativePath);

            // 拷贝文件
            CopyFile(files[i], path);
        }
    }

    public static string Normalize(string path)
    {
        return path.Replace("\\", "/");
    }
}
