namespace QBotDotnet.GoCqHttp;

public abstract class CqApi
{
    // <summary>
    /// 该api的昵称
    /// </summary>
    [Ignore]
    public abstract string ApiName { get; }

    /// <summary>
    /// 对应的回应的类型
    /// </summary>
    [Ignore]
    public abstract Type ApiResultDataType { get; }
}