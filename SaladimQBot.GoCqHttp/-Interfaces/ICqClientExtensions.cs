namespace SaladimQBot.GoCqHttp;


public static class ICqClientExtensions
{
    internal static async Task<CqApiCallResult> CallApiWithCheckingAsync(this ICqClient client, CqApi api, bool implicitly)
    {
        var result = await client.CallApiAsync(api);
        if (result is null) throw new CqApiCallFailedException(client, implicitly, api);
        if (result.Data is null) throw new CqApiCallFailedException(client, implicitly, api, result);
        return result;
    }

    internal static Task<CqApiCallResult> CallApiImplicityWithCheckingAsync(this ICqClient client, CqApi api)
        => client.CallApiWithCheckingAsync(api, true);

    public static Task<CqApiCallResult> CallApiWithCheckingAsync(this ICqClient client, CqApi api)
        => client.CallApiWithCheckingAsync(api, false);
}