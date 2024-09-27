using Newtonsoft.Json;
using System.IO;
using System.Text;

public static class JsonHelpers {
    public static string Serialize(object obj) {
        var serializer = new JsonSerializer();
        var sb = new StringBuilder();
        using var tw = new StringWriter(sb);
        serializer.Serialize(tw, obj);
        return sb.ToString();
    }

    public static T Deserialize<T>(string text) {
        var serializer = new JsonSerializer();
        using var tr = new JsonTextReader(new StringReader(text));

        return serializer.Deserialize<T>(tr);
    }
}