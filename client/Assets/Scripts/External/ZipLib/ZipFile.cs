// ZipFile.cs
// Created by xiaojl, Sep/17/2022
// Zip文件类
// 实现Zip文件的增删改及遍历等功能

using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using InternalZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace ZipLib
{
    public class ZipFile : IDisposable
    {
        private InternalZipFile internalZipFile;

        public ZipFile(string path, bool autoCreate = false)
        {
            if (! File.Exists(path) && autoCreate)
            {
                internalZipFile = InternalZipFile.Create(path);
                return;
            }
            internalZipFile = new InternalZipFile(path);
        }

        public ZipFile(byte[] fileBytes)
        {
            MemoryStream ms = new MemoryStream(fileBytes);
            internalZipFile = new InternalZipFile(ms);
        }

        public ZipFile(Stream stream)
        {
            internalZipFile = new InternalZipFile(stream);
        }

        ~ZipFile()
        {
            Dispose();
        }

        public bool IsOpen()
        {
            return internalZipFile != null;
        }

        public void Dispose()
        {
            if (null != internalZipFile)
            {
                internalZipFile.Close();
            }
        }

        // 文件数量
        public long Count
        {
            get
            {
                return internalZipFile.Count;
            }
        }

        // 文件名
        public string Name
        {
            get
            {
                return internalZipFile.Name;
            }
        }

        // 文件信息
        public ZipEntryInfo this[int index]
        {
            get
            {
                return new ZipEntryInfo(internalZipFile[index]);
            }
        }

        // 读取文件
        public byte[] ReadFile(int index)
        {
            if (-1 == index)
                return null;

            ZipEntry ze = internalZipFile[index];
            if (null == ze)
                return null;

            using (Stream s = internalZipFile.GetInputStream(ze))
            {
                if (null == s)
                {
                    return null;
                }

                byte[] fileBytes = new byte[1024];
                int count = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    while (0 != (count = s.Read(fileBytes, 0, fileBytes.Length)))
                    {
                        ms.Write(fileBytes, 0, count);
                    }
                    return ms.ToArray();
                }
            }
        }

        // 读取文件
        public byte[] ReadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            int index = internalZipFile.FindEntry(fileName, true);
            return ReadFile(index);
        }

        // 导出文件
        public bool ExportFile(string fileName, Stream stream)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var index = internalZipFile.FindEntry(fileName, true);

            if (-1 == index)
                return false;

            var ze = internalZipFile[index];
            if (null == ze)
                return false;

            using (var reader = internalZipFile.GetInputStream(ze))
            {
                if (null == reader)
                    return false;

                var buffer = new byte[128 * 1024];
                var count  = 0;

                while (0 != (count = reader.Read(buffer, 0, buffer.Length)))
                {
                    stream.Write(buffer, 0, count);
                }
            }

            return true;
        }

        // 写文件
        private void WriteFile(string fileName, Stream fileStream)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            internalZipFile.BeginUpdate();
            internalZipFile.Add(new StreamDataSource(fileStream), fileName);
            internalZipFile.CommitUpdate();
        }

        // 写文件
        public void WriteFile(string fileName, byte[] fileBytes)
        {
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                WriteFile(fileName, ms);
            }
        }

        // 写文件
        public void WriteFile(string fileName, string fileContent, Encoding encoding)
        {
            byte[] fileBytes = encoding.GetBytes(fileContent);
            if (null == fileBytes || fileBytes.Length <= 0)
                fileBytes = new byte[] {};

            WriteFile(fileName, fileBytes);
        }

        // 删除文件
        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            internalZipFile.BeginUpdate();
            internalZipFile.Delete(fileName);
            internalZipFile.CommitUpdate();
        }

        // 获取文件信息
        public ZipEntryInfo GetFileInfo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            ZipEntry ze = internalZipFile.GetEntry(fileName);
            return ze == null ? null : new ZipEntryInfo(ze);
        }

        // 是否存在文件
        public bool ExistFile(string name)
        {
            ZipEntryInfo info = GetFileInfo(name);
            return info != null && info.IsFile;
        }

        // 遍历指定目录
        public void ListFile(string path, Action<ZipEntryInfo> callbackFunc)
        {
            foreach (ZipEntry ze in internalZipFile)
            {
                if (path != "/" && (! string.IsNullOrEmpty(path) && "." != path && ! IsChildOrSelf(path, ze.Name)))
                    continue;

                callbackFunc(new ZipEntryInfo(ze));
            }
        }

        // 确保目录存在，不存在则创建
        private void _AssureDirectory(string path)
        {
            internalZipFile.BeginUpdate();
            internalZipFile.AddDirectory(path);
            internalZipFile.CommitUpdate();
        }

        // 确保目录是否存在，不存在则创建之
        public void AssureDirectory(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
                return;

            string[] pathList = dirPath.Split(new char[] { '/', '\\' },
                StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pathList.Length; i++)
            {
                string path = pathList[i];
                if (path == "" || path == ".")
                    continue;

                sb.Append(path);
                _AssureDirectory(sb.ToString());
                sb.Append("/");
            }
        }

        // 是否存在文件夹
        public bool ExistDirectory(string path)
        {
            ZipEntryInfo info = GetPathInfo(path);
            return info != null && info.IsDirectory;
        }

        // 获取目录信息
        public ZipEntryInfo GetPathInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            ZipEntry ze = internalZipFile.GetEntry(path);
            return ze == null ? null : new ZipEntryInfo(ze);
        }

        // 删除目录
        public void DeleteDirectory(string path)
        {
            throw new NotSupportedException("Zip文件夹不支持删除目录操作。");
        }

        #region Private

        // 是否是子目录或自身
        private bool IsChildOrSelf(string path, string checkPath)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(checkPath))
                return false;

            if (path == checkPath)
                return true;

            if (checkPath.Length < path.Length || ! checkPath.StartsWith(path))
                return false;

            if (! path.EndsWith("/") && ! path.EndsWith("\\"))
            {
                char c = checkPath[path.Length];
                if ('/' != c && '\\' != c)
                    return false;
            }

            return true;
        }

        #endregion Private

        #region StreamDataSource
        // Stream 数据源
        // 支持从Byte数组或字符串中压缩
        private class StreamDataSource : IStaticDataSource
        {
            Stream stream = null;

            // 从流中创建实例
            public StreamDataSource(Stream stream)
            {
                this.stream = stream;
            }

            // 从byte数组中创建实例
            public StreamDataSource(byte[] buffer)
                : this(new MemoryStream(buffer))
            {
            }

            // 从字符串中创建实例
            public StreamDataSource(string str, Encoding encoding)
                : this(encoding.GetBytes(str))
            {
            }

            #region IStaticDataSource implementation

            public Stream GetSource()
            {
                return stream;
            }

            #endregion
        }
        #endregion StreamDataSource
    }

    // Zip文件信息
    public class ZipEntryInfo
    {
        private ZipEntry entry;

        internal ZipEntryInfo(ZipEntry ze)
        {
            this.entry = ze;
        }

        // 文件/路径名
        public string Name
        {
            get
            {
                return entry.Name;
            }
        }

        // 原始文件大小
        public long RawSize
        {
            get
            {
                return entry.Size;
            }
        }

        // 文件大小
        public long Size
        {
            get
            {
                return entry.CompressedSize;
            }
        }

        // 是否文件
        public bool IsFile
        {
            get
            {
                return entry != null && entry.IsFile;
            }
        }

        // 是否目录
        public bool IsDirectory
        {
            get
            {
                return entry != null && entry.IsDirectory;
            }
        }

        // 注释
        public string Comment
        {
            get
            {
                return entry.Comment;
            }
        }

        // 修改时间
        public DateTime ModifiedTime
        {
            get
            {
                return entry.DateTime;
            }
        }

        // 在文件中的索引
        public long Index
        {
            get
            {
                return entry.ZipFileIndex;
            }
        }
    }
}
