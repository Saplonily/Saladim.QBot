namespace QBotDotnet.GoCqHttp;

/// <summary>
/// 当读取go-cqhttp上报时失败抛出的异常，大概率原因出自本类库，如果你被抛出了此异常请提交issue
/// </summary>
public class CqPostInvalidException : Exception
{
    public CqPostInvalidException(string msg) : base(msg)
    {
    }
}