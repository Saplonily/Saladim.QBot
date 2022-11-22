namespace SaladimQBot.GoCqHttp;

public class CqApiCallResult
{
    [Name("status")]
    public string Status { get; set; } = default!;
    [Name("retcode")]
    public int ReturnCode { get; set; } = default!;
    [Name("msg")]
    public string? Message { get; set; } = default!;
    [Name("wording")]
    public string? Wording { get; set; } = default!;
    [Name("echo")]
    public string Echo { get; set; } = default!;

    [Ignore]
    public CqApiCallResultData? Data { get; set; } = null;
}