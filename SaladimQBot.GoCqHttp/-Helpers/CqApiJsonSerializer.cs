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
    /// 给定api, 序列化为一个参数的Json节点
    /// </summary>
    /// <param name="api">要序列化的api</param>
    /// <param name="echo">echo,建议填写一般短时间内不会重复的值</param>
    /// <returns>序列化得到的json节点</returns>
    public static JsonObject SerializeApiParamsToNode(CqApi api)
        => (JsonObject)JsonSerializer.SerializeToNode(api, api.GetType(), CqJsonOptions.Instance)!;

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