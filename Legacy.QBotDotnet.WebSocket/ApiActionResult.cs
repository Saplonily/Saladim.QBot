using System.Diagnostics;
using System.Text.Json;

namespace QBotDotnet.GoCqHttp;

public class ApiActionResult
{
    public ApiCallStatus Status { get; private set; }
    public int ReturnCode { get; private set; } = -1;
    public string? ErrorMessage { get; private set; }
    public string? ErrorWording { get; private set; }
    public ApiActionResultData Data { get; private set; } = null!;
    public class ApiActionResultData
    {
        public Dictionary<string, JsonElement> RawDictionary { get => dic; }

        private Dictionary<string, JsonElement> dic;
        public ApiActionResultData(Dictionary<string, JsonElement> dic)
        {
            this.dic = dic;
        }
        public string? String(string key) => GetElement(key).GetString();
        public string String(string key, string @default) => String(key) ?? @default;
        public int? Int(string key) => GetElement(key).GetInt32();
        public uint? Uint(string key) => GetElement(key).GetUInt32();
        internal JsonElement GetElement(string key) => dic[key];
    }

    private ApiActionResult() { }
    internal static ApiActionResult GetFrom(JsonElement je)
    {
        var result = new ApiActionResult();
        result.LoadFrom(je);
        return result;
    }

    internal void LoadFrom(JsonElement je)
    {
        try
        {
            Status = (je.GetProperty("status").GetString() ?? "").ToLower() switch
            {
                "ok" => ApiCallStatus.Ok,
                "failed" => ApiCallStatus.Failed,
                "async" => ApiCallStatus.Async,
                _ => ApiCallStatus.Invalid
            };
            ReturnCode = je.GetProperty("retcode").GetInt32();
            if (Status == ApiCallStatus.Failed)
            {
                ErrorMessage = je.GetProperty("msg").GetString();
                ErrorWording = je.GetProperty("wording").GetString();
            }
            je.TryGetProperty("data", out JsonElement dataJE);
            if (dataJE.ValueKind != JsonValueKind.Null)
            {
                var dic = dataJE.Deserialize<Dictionary<string, JsonElement>>();
                Debug.Assert(dic is not null);
                Data = new(dic);
            }
        }
        catch (KeyNotFoundException e)
        {
            throw new Internal.CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }

    public enum ApiCallStatus
    {
        Invalid,
        Ok,
        Failed,
        Async
    }
}
/*{
    "status": "状态, 表示 API 是否调用成功, 如果成功, 则是 OK, 其他的在下面会说明",
    "retcode": 0,
    "msg": "错误消息, 仅在 API 调用失败式有该字段",
    "wording": "对错误的详细解释(中文), 仅在 API 调用失败时有该字段",
    "data": {
        "响应数据名": "数据值",
        "响应数据名2": "数据值",
    },
    "echo": "'回声', 如果请求时指定了 echo, 那么响应也会包含 echo"
}*/