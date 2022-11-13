// Json.cs
// Created by liujj2 Aug/28/2017
// Json接口

using JsonFx.Json;
using System.Text;

public class Json
{
    public static string SaveTo(object obj)
    {
        var stream   = new StringBuilder();
        var settings = new JsonWriterSettings();

        settings.PrettyPrint = true;
        settings.Tab         = "    ";
        settings.NewLine     = "\n";

        var Writer = new JsonWriter(stream, settings);
        Writer.Write(obj);

        return stream.ToString();
    }

    public static T RestoreFrom<T>(string content)
    {
        return JsonReader.Deserialize<T>(content);
    }
}
