using System.Security.Cryptography;

public class Md5
{
    public static byte[] Encode(byte[] bytes)
    {
        byte[] output;

        // 计算md5
        using (var md5 = new MD5CryptoServiceProvider())
        {
            output = md5.ComputeHash(bytes);
        }

        return output;
    }
}