using System.Text.Json;
using System.Text.Json.Nodes;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// <para>请使用<see cref="Name"/>等特性指定序列化结果</para>
/// </summary>
public static class CqApiJsonSerializer
{
    /// <summary>
    /// 使用给定的echo序列化一个api为json字符串
    /// </summary>
    /// <param name="api">要序列化的api</param>
    /// <param name="echo">echo,建议填写一般短时间内不会重复的值</param>
    /// <returns>序列化得到的json字符串</returns>
    public static string SerializeApi(CqApi api, string echo)
    {
        /**序列化出来的格式样例:
          {
	        "action": "actionname",
	        "echo": "somerandomecho",
	        "params": {
		        "param_a": "value",
		        "param_b": "value_b",
                "no_cache": true,
	        }
          }
        */
        //最外层
        JsonObject rootObj = new(new Dictionary<string, JsonNode?>()
        {
            [StringConsts.ActionProperty] = JsonValue.Create(api.ApiName),
            [StringConsts.ApiEchoProperty] = JsonValue.Create(echo),
        });
        //params json对象
        JsonObject paramNode =
            (JsonObject)JsonSerializer.SerializeToNode(api, api.GetType(), CqJsonOptions.Instance)!;
        //组装起来然后转成string
        rootObj.Add(StringConsts.ParamsProperty, paramNode);
        string result = rootObj.ToJsonString(CqJsonOptions.Instance);
        return result;
    }

    public static CqApiCallResult? DeserializeApiResult(string source, CqApi sourceApi)
        => DeserializeApiResult(JsonDocument.Parse(source), sourceApi);
    public static CqApiCallResult? DeserializeApiResult(JsonDocument sourceDoc, CqApi sourceApi)
    {
        CqApiCallResult? raw = JsonSerializer.Deserialize<CqApiCallResult?>(sourceDoc, CqJsonOptions.Instance);
        if (sourceApi.ApiResultDataType is null || raw is null) return raw;
        CqApiCallResult updated = raw;
        var dataObj = JsonSerializer.Deserialize(
            sourceDoc.RootElement.GetProperty(StringConsts.DataProperty),
            sourceApi.ApiResultDataType,
            CqJsonOptions.Instance
            );
        if (dataObj is null) return raw;
        var data = dataObj.Cast<CqApiCallResultData>();
        data.ResultIn = raw;
        updated.Data = data;
        return updated;
    }

    public static CqApiCallResult? DeserializeRawApiResult(string source)
        => DeserializeRawApiResult(JsonDocument.Parse(source));

    public static CqApiCallResult? DeserializeRawApiResult(JsonDocument sourceDoc)
        => JsonSerializer.Deserialize<CqApiCallResult?>(sourceDoc, CqJsonOptions.Instance);
}