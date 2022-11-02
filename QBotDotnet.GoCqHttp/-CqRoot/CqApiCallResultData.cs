namespace QBotDotnet.GoCqHttp;

public class CqApiCallResultData
{
    public CqApiCallResult ResultIn { get; internal set; } = default!;
    public CqApiCallResultData()
    {
    }
    public CqApiCallResultData(CqApiCallResult resultIn)
    {
        ResultIn = resultIn;
    }
}