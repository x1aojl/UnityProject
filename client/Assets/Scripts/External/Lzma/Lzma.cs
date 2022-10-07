using System;
using System.Runtime.InteropServices;

public class Lzma
{
    public enum Status
    {
        SZ_OK                = 0,
        SZ_ERROR_DATA        = 1,
        SZ_ERROR_MEM         = 2,
        SZ_ERROR_CRC         = 3,
        SZ_ERROR_UNSUPPORTED = 4,
        SZ_ERROR_PARAM       = 5,
        SZ_ERROR_INPUT_EOF   = 6,
        SZ_ERROR_OUTPUT_EOF  = 7,
        SZ_ERROR_READ        = 8,
        SZ_ERROR_WRITE       = 9,
        SZ_ERROR_PROGRESS    = 10,
        SZ_ERROR_FAIL        = 11,
        SZ_ERROR_THREAD      = 12,
        SZ_ERROR_ARCHIVE     = 16,
        SZ_ERROR_NO_ARCHIVE  = 17,
    }

    public static void Encode(string inFile, string destFile)
    {
        Status status;

        // 开始压缩
        if ((status = (Status)LzmaEncodeFile(inFile, destFile, 5, (1 << 24), 3, 0, 2, 32, 2, IntPtr.Zero)) != Status.SZ_OK)
            Diagnostics.RaiseException("Lzma compress failed, status={0}", status);
    }

    public static void Decode(string inFile, string destFile)
    {
        Status status;

        if ((status = (Status)LzmaDecodeFile(inFile, destFile, IntPtr.Zero)) != Status.SZ_OK)
            Diagnostics.RaiseException("Lzma decompress failed, status={0}", status);
    }

    [DllImport(LZMALIB, CallingConvention = CallingConvention.Cdecl)]
    extern private static int LzmaEncodeFile(
        string inFile,
        string outFile,
        int level,         // 0 <= level <= 9, default = 5
        uint dictSize,     // default = (1 << 24)
        int lc,            // 0 <= lc <= 8, default = 3
        int lp,            // 0 <= lp <= 4, default = 0
        int pb,            // 0 <= pb <= 4, default = 2
        int fb,            // 5 <= fb <= 273, default = 32
        int numThreads,    // 1 or 2, default = 2
        IntPtr onProgress);

    [DllImport(LZMALIB, CallingConvention = CallingConvention.Cdecl)]
    extern private static int LzmaDecodeFile(string inFile, string outFile, IntPtr onProgress);

    // iOS平台
#if UNITY_IPHONE && !UNITY_EDITOR
    const string LZMALIB = "__Internal";
    // MacOS editor平台
#elif UNITY_EDITOR_OSX
    const string LZMALIB = "lzma";
    // Android & windows 平台
#else
    const string LZMALIB = "lzma";
#endif
}