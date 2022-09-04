using System;

public partial class ResPacker
{
    private static void BuildPacket()
    {
        try
        {
            // 打包
            BuildPacketProtected();
        }
        catch
        {
            throw new Exception("Build release packet failed.");
        }
    }

    private static void BuildPacketProtected()
    {
        // 解析命令行参数
        var arguments = GetCommandLineArgs();

        // 打完整包
        if (arguments[0] == "release")
        {
            BuildFullPacket(
                arguments[1],
                arguments[2],
                GetBuildPlatformEnum(arguments[3]));
        }
    }

    private static string[] GetCommandLineArgs()
    {
        // 获取命令行参数
        var arguments = Environment.GetCommandLineArgs();

        // 解析命令行参数
        for (var i = 1; i < arguments.Length; ++i)
        {
            // 获取当前要解析的参数
            var argument = arguments[i];

            // 分析参数
            switch (argument)
            {
                // Unity相关
                case "-quit":
                case "-batchmode":
                case "-nographics":
                    continue;
                case "-projectPath":
                case "-executeMethod":
                case "-logFile":
                    ++i;
                    continue;
                // 打完整包
                case "--buildRelease":
                    if (arguments.Length <= i + 3)
                        throw new Exception("buildRelease command need 4 arguments.");

                    return new string[] {
                        "release",
                        arguments[i + 1], // output
                        arguments[i + 2], // version
                        arguments[i + 3], // platform
                    };
                default:
                    throw new Exception(string.Format("Unkown command argument. arg={0}", argument));
            }
        }

        // 参数解析失败
        return null;
    }
}
