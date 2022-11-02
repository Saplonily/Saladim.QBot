using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

public class CqApiCallFailedException : Exception
{
    protected static string GenerateMessage(CqApiCallResult echo)
    {
        var s = echo;
        return $@"go-cqhttp API call failed. " +
            $@"Error message: ""{s.Message}"". " +
            $@"Word: ""{s.Wording}"". " +
            $@"Status: ""{s.Status}"". " +
            $@"Return code: {s.ReturnCode}";

    }
    public CqApiCallFailedException(CqApiCallResult echo) : base(GenerateMessage(echo))
    {
    }
}