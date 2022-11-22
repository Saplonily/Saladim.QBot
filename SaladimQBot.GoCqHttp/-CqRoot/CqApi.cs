namespace SaladimQBot.GoCqHttp;

public abstract class CqApi
{
    // <summary>
    /// 该api的昵称
    /// </summary>
    public abstract string ApiName { get; }

    /// <summary>
    /// 对应的回应的类型
    /// </summary>
    public abstract Type? ApiResultDataType { get; }
}