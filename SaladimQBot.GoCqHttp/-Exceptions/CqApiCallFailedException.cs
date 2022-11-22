using System.Text;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class CqApiCallFailedException : ClientException
{
    public CqApi ApiInstance { get; protected set; }

    public bool IsImplicit { get; protected set; }

    public CqApiCallFailedException(
        IClient client,
        bool isForImplicit,
        CqApi apiInstance,
        CqApiCallResult? result = null,
        string? message = null,
        Exception? innerException = null
        )
        : base(
            client,
            isForImplicit ? ExceptionType.ImplicitApiCallFailed : ExceptionType.ExplicitApiCallFailed,
            GenerateMessage(apiInstance, message, result),
            innerException
            )
    {
        ApiInstance = apiInstance;
        IsImplicit = isForImplicit;
    }

    public static string GenerateMessage(CqApi apiInstance, string? message, CqApiCallResult? result)
    {
        StringBuilder sb = new();
        sb.Append($"Api name is {apiInstance.ApiName}.");
        if (result is not null)
        {
            sb.Append(
                $@"Error message: ""{result.Message}"". " +
                $@"Word: ""{result.Wording}"". " +
                $@"Status: ""{result.Status}"". " +
                $@"Return code: {result.ReturnCode}"
                );
        }
        else
        {
            sb.Append(
                "No CqApiCallResult instance gotten."
                );
        }
        if (message is not null) sb.Append(message);
        return sb.ToString();
    }
}