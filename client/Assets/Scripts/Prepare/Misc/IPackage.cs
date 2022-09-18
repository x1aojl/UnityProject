using System.Collections.Generic;
using System.IO;
using ZipLib;

public interface IPackage
{
    void     Close();
    byte[]   ReadFile(string path);
    void     WriteFile(string path, byte[] bytes);
    void     DeleteFile(string path);
    void     DeleteFiles(string path);
    bool     IsFileExist(string path);
    string[] GetFiles(string path);
    string   GetFullPath(string path);
    int      GetFileLength(string path);
    bool     ExportFile(string path, Stream stream);
}

// 文件系统包
public class FSPackage : IPackage
{
    // 获取根目录
    public string Root { get { return _root; } }

    // 构造函数
    public FSPackage(string dir)
    {
        this._root = dir;
    }

    // 关闭
    public void Close()
    {
        // Do nothing
    }

    // 读取文件
    public byte[] ReadFile(string path)
    {
        return _ReadFile(GetFullPath(path));
    }

    // 写入文件
    public void WriteFile(string path, byte[] bytes)
    {
        _WriteFile(GetFullPath(path), bytes);
    }

    // 删除文件
    public void DeleteFile(string path)
    {
        _DeleteFile(GetFullPath(path));
    }

    // 通过绝对路径删除所有文件
    // --文件夹结构保留
    public void DeleteFiles(string path)
    {
        var fullpath = GetFullPath(path);
        if (File.Exists(fullpath))
            _DeleteFile(fullpath);
        else
            _DeleteFiles(fullpath);
    }

    // 判断文件是否存在
    public bool IsFileExist(string path)
    {
        var fullPath = GetFullPath(path);
        return _IsFileExist(fullPath);
    }

    // 读取所有文件
    public string[] GetFiles(string path)
    {
        var fullPath = GetFullPath(path);
        if (_IsFileExist(fullPath))
            return new string[1] { path };

        // 获取目录下的所有文件
        var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
            files[i] = GetRelativePath(files[i]);

        return files;
    }

    // 获取相对路径
    public string GetRelativePath(string path)
    {
        if (_root.Length == 0)
            return _root;

        return path.Substring(_root.Length + 1);
    }

    // 获取绝对路径
    public string GetFullPath(string path)
    {
        if (path.Length == 0)
            return _root;

        return _root + "/" + path;
    }

    // 获取文件长度
    public int GetFileLength(string path)
    {
        var fullPath = GetFullPath(path);
        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Exists)
            return (int)fileInfo.Length;

        return -1;
    }

    // 导出文件
    public bool ExportFile(string path, Stream stream)
    {
        var fullPath = GetFullPath(path);
        if (! File.Exists(fullPath))
            return false;

        using (var reader = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            var buffer = new byte[128 * 1024];
            var count = 0;

            while (0 != (count = reader.Read(buffer, 0, buffer.Length)))
                stream.Write(buffer, 0, count);
        }

        return true;
    }

    #region Private

    // 读取文件
    private byte[] _ReadFile(string fullPath)
    {
        if (! File.Exists(fullPath))
            return null;

        return File.ReadAllBytes(fullPath);
    }

    // 写入文件
    private void _WriteFile(string fullPath, byte[] bytes)
    {
        var dir = Path.GetDirectoryName(fullPath);

        // 如果目录不存在，创建目录
        if (! Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        FileUtil.Write(fullPath, bytes);
    }

    // 删除文件
    private void _DeleteFile(string fullPath)
    {
        if (! File.Exists(fullPath))
            return;

        FileUtil.Delete(fullPath);
    }

    // 通过绝对路径删除所有文件
    // --文件夹结构保留
    private void _DeleteFiles(string fullPath)
    {
        if (! Directory.Exists(fullPath))
            return;

        var dirs = Directory.GetDirectories(fullPath);
        foreach (var dir in dirs)
            _DeleteFiles(dir);

        var files = Directory.GetFiles(fullPath);
        foreach (var file in files)
            _DeleteFile(file);
    }

    // 判断文件是否存在
    private bool _IsFileExist(string fullPath)
    {
        return File.Exists(fullPath);
    }

    #endregion

    // 根目录
    private string _root;
}

public class ZipPackage : IPackage
{
    public ZipPackage(string path)
    {
        this._zip = new ZipFile(path);
    }

    public ZipPackage(byte[] bytes)
    {
        this._zip = new ZipFile(bytes);
    }

    public void Close()
    {
        this._zip.Dispose();
        this._zip = null;
    }

    public virtual byte[] ReadFile(string path)
    {
        return this._zip.ReadFile(GetFullPath(path));
    }

    public virtual void WriteFile(string path, byte[] bs)
    {
        throw new System.NotSupportedException();
    }

    public virtual void DeleteFile(string path)
    {
        throw new System.NotSupportedException();
    }

    public virtual bool IsFileExist(string path)
    {
        return this._zip.ExistFile(GetFullPath(path));
    }

    public int GetFileLength(string path)
    {
        throw new System.NotSupportedException();
    }

    public virtual void DeleteFiles(string path)
    {
        throw new System.NotSupportedException();
    }

    public virtual string[] GetFiles(string path)
    {
        var files = new List<string>();

        // 遍历所有文件
        var fullPath = GetFullPath(path);
        this._zip.ListFile(fullPath, (zipEntryInfo) =>
        {
            // 只关心文件
            if (! zipEntryInfo.IsFile)
                return;

            files.Add(GetRelativePath(zipEntryInfo.Name));
        });

        return files.ToArray();
    }

    public virtual string GetFullPath(string path)
    {
        return path;
    }

    protected virtual string GetRelativePath(string path)
    {
        return path;
    }

    public bool ExportFile(string path, Stream stream)
    {
        return this._zip.ExportFile(GetFullPath(path), stream);
    }

    private ZipFile _zip;
}

public class AndroidPackage : ZipPackage
{
    public AndroidPackage(string path, string root)
        : base(path)
    {
        _root = root;
    }

    public override string GetFullPath(string path)
    {
        return PathUtil.Combine(_root, path);
    }

    protected override string GetRelativePath(string path)
    {
        return PathUtil.GetRelativePath(_root, path);
    }

    // 资源在安卓zip下的相对根目录
    private string _root;
}