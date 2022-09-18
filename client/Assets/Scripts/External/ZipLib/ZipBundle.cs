using ICSharpCode.SharpZipLib.Zip;
using System.IO;

public class ZipBundle
{
    // 获取zip entry
    public ZipEntry this[int index]
    {
        get { return m_ZipFile[index]; }
    }

    public ZipBundle(string path, FileAccess access)
    {
        // 只读方式打开zip文件
        if (access == FileAccess.Read)
            m_ZipFile = new ZipFile(path);
        // 创建新的zip文件
        else if (access == FileAccess.Write)
            m_ZipFile = ZipFile.Create(path);
        // 不能以可读写的方式打开zip文件
        else
            Diagnostics.RaiseException("Invalid access flag.");
    }

    public void Dispose()
    {
        // 关闭zip文件
        if (m_ZipFile != null)
            m_ZipFile.Close();
    }

    public void Write(string path, byte[] content, CompressionMethod method = CompressionMethod.Stored)
    {
        var stream = new DataSourceStream(content);

        m_ZipFile.BeginUpdate();
        m_ZipFile.Add(stream, path, CompressionMethod.Stored);
        m_ZipFile.CommitUpdate();
    }

    public void WriteDirectory(string path)
    {
        Diagnostics.Assert(
            Directory.Exists(path),
            "Failed pack zip bundle. source directory({0}) not exist.", path
        );

        var files = PathUtil.GetFiles(path, "*.*", SearchOption.AllDirectories);

        m_ZipFile.BeginUpdate();

        for (var i = 0; i < files.Length; ++i)
        {
            var key   = PathUtil.GetRelativePath(path, files[i]);
            var value = File.ReadAllBytes(files[i]);

            var stream = new DataSourceStream(value);
            m_ZipFile.Add(stream, key, CompressionMethod.Stored);
        }

        m_ZipFile.CommitUpdate();
    }

    public void DeleteFile(string path)
    { }

    public static void Pack(string sourceDir, string destPath)
    {
        Diagnostics.Assert(
            Directory.Exists(sourceDir),
            "Failed pack zip bundle. source directory({0}) not exist.", sourceDir
        );

        // 创建新的zip文件
        var bundle = new ZipBundle(destPath, FileAccess.Write);

        bundle.WriteDirectory(sourceDir);
        bundle.Dispose();
    }

    public static void Unpack(string sourceDir, string destDir)
    { }

    private ZipFile m_ZipFile;

    private class DataSourceStream : IStaticDataSource
    {
        public DataSourceStream(byte[] content)
        {
            m_Stream = new MemoryStream(content);
        }

        public Stream GetSource()
        {
            return m_Stream;
        }

        private Stream m_Stream;
    }
}