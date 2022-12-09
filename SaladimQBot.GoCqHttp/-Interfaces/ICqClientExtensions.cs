namespace SaladimQBot.GoCqHttp;


public static class ICqClientExtensions
{
    internal static async Task<CqApiCallResult> CallApiWithCheckingAsync(this CqClient client, CqApi api, bool implicitly)
    {
        var result = await client.CallApiAsync(api);
        if (result is null) throw new CqApiCallFailedException(client, implicitly, api);
        if (api.ApiResultDataType is not null && result.Data is null)
            throw new CqApiCallFailedException(client, implicitly, api, result);
        return result;
    }

    internal static Task<CqApiCallResult> CallApiImplicitlyWithCheckingAsync(this CqClient client, CqApi api)
        => client.CallApiWithCheckingAsync(api, true);

    internal static async Task<(CqApiCallResult, T)> CallApiImplicitlyWithCheckingAsync<T>(this CqClient client, CqApi api)
        where T : CqApiCallResultData
    {
        var r = await CallApiImplicitlyWithCheckingAsync(client, api);
        return (r, (T)r.Data!);
    }

    public static Task<CqApiCallResult> CallApiWithCheckingAsync(this CqClient client, CqApi api)
        => client.CallApiWithCheckingAsync(api, false);

    public static async Task<(CqApiCallResult, T)> CallApiWithCheckingAsync<T>(this CqClient client, CqApi api)
        where T : CqApiCallResultData
    {
        var r = await CallApiWithCheckingAsync(client, api);
        return (r, (T)r.Data!);
    }
}