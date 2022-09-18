using System;
using System.Collections;
using System.IO;

public partial class Bootstrap
{
    // 从初始安装包中导出文件
    protected IEnumerator ExportFromReleasePkg()
    {
        // zip下需要解压的所有文件
        var files = _releasePkg.GetFiles("");
        if (files == null || files.Length == 0)
            yield break;

        Exception error = null;
        bool success = true;

        try
        {
            // 拷贝操作
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                // 获取导出文件地址
                var writePath = _patchPkg.GetFullPath(file);

                // 如果目录不存在, 创建目录
                var dir = Path.GetDirectoryName(writePath);
                if (! Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var write = new FileStream(writePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    // 解压zip文件到目标路径下
                    success = _releasePkg.ExportFile(file, write);
                }

                // 导出失败
                if (! success)
                    break;
            }
        }
        catch (Exception e)
        {
            error = e;
        }

        // 异常提示框
        if (error != null || ! success)
        {
            var message = error != null ? error.Message : "release export failed";

            yield return ShowErrorBox(
                string.Format("释放安装包文件失败, error:{0}", message),
                string.Format("释放安装包文件失败, 错误码:{0}", E_FILEWRITEERROR)
                );
        }
    }
}