using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SaladimQBot.GoCqHttp;

public static class CqJsonOptions
{
    public static JsonSerializerOptions Instance { get; }

    static CqJsonOptions()
    {
        var o = new JsonSerializerOptions()
        {
            IgnoreReadOnlyProperties = true,
            Converters =
            {
                new CqPostJsonConverter(),
                new CqMessagePostJsonConverter(),
                new CqNoticePostJsonConverter(),
                new CqNotifyNoticePostJsonConverter(),
            },
            //不要转义中文什么的
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        };

        ConfigureDebug(o);

        Instance = o;
    }

    [Conditional("DEBUG")]
    private static void ConfigureDebug(JsonSerializerOptions o)
    {
        o.WriteIndented = true;
    }
}