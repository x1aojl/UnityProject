using System;
using System.Collections;
using System.IO;

public partial class Bootstrap
{
    // 安全删除某个文件夹下的文件
    protected IEnumerator DeleteFilesProtected(IPackage pkg, string path)
    {
        Exception error = null;

        try
        {
            pkg.DeleteFiles(path);
        }
        catch (Exception e)
        {
            error = e;
        }

        if (error != null)
        {
            yield return ShowErrorBox(
                string.Format("文件夹删除异常, path:{0}, error:{1}", path, error.Message),
                string.Format("文件夹删除异常, 错误码:{0}", E_DELETEFILEFAIL)
                );
        }
    }

    // 安全写入文件
    protected IEnumerator WriteFileProtected(IPackage pkg, string path, byte[] bytes)
    {
        Exception error = null;

        try
        {
            pkg.WriteFile(path, bytes);
        }
        catch (Exception e)
        {
            error = e;
        }

        if (error != null)
        {
            yield return ShowErrorBox(
                string.Format("文件写入异常, path:{0}, error:{1}", path, error.Message),
                string.Format("文件写入异常, 错误码:{0}", E_FILEWRITEERROR)
                );
        }
    }

    // 删除目录并重新创建
    protected IEnumerator DeleteAndCreateDirectory(string path)
    {
        Exception error = null;

        try
        {
            // 清理目录
            FileUtil.CheckAndCreateDirectory(path, true);
        }
        catch (Exception e)
        {
            error = e;
        }

        if (error != null)
        {
            yield return ShowErrorBox(
                string.Format("目录删除异常, path:{0}, error:{1}", path, error.Message),
                string.Format("目录删除异常, 错误码:{0}", E_DELETEDIRFAIL)
                );
        }
    }

    protected string ComputeMd5(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        var md5 = Md5.Encode(bytes);

        // 转换成可读性更好的字符串
        return BitConverter.ToString(md5).Replace("_", "").ToLowerInvariant();
    }

    // 占用磁盘大小描述
    protected string CalcDiskSpaceFormat(int size)
    {
        var Mkb = 1 * 1024 * 1024;
        var kb = 1 * 1024;

        if (size > Mkb)
            return string.Format("{0}M", (size / (float)Mkb).ToString("f1"));
        else if (size > kb)
            return string.Format("{0}KB", (size / (float)kb).ToString("f1"));
        else
            return string.Format("{0}B", size);
    }

    // 空余磁盘大小
    protected bool CheckDiskFreeSpace(long size)
    {
        return true;
    }
}