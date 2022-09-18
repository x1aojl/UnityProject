public partial class Bootstrap
{
    public const int E_OK                = 0; // ok
    public const int E_CLOSEFAIL         = 1; // 关闭stream异常
    public const int E_HTTPCONNFAIL      = 2; // http连接失败
    public const int E_HTTPDISCONNECT    = 3; // http下载出错
    public const int E_DELETEDIRFAIL     = 4; // 删除目录失败
    public const int E_DELETEFILEFAIL    = 5; // 删除文件失败
    public const int E_FILEMD5NOTMATCH   = 6; // 文件md5不匹配
    public const int E_DOWNLOADDATAERROR = 7; // 下载数据异常
    public const int E_FILEWRITEERROR    = 8; // 写文件失败
    public const int E_DECODEFILEFAIL    = 9; // 解压失败
}