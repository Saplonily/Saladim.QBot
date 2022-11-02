namespace QBotDotnet.Exceptions;

/// <summary>
/// 接受上报时，上报信息读取失败
/// <para>可能原因</para>
/// <para>1. 本项目读取问题(可能性大)</para>
/// <para>2. 外部上报出错</para>
/// </summary>
public class PostLoadFailedException : Exception
{
    private readonly string argName = string.Empty;
    private readonly string? innerText = null;
    public override string Message
    {
        get
        {
            string str = $"{argName}";
            if (innerText != null) str += $"\nInner description: {innerText}";
            return str;
        }
    }
    public PostLoadFailedException(string argName, Exception? innerException = null, string? text = null)
        : base(null, innerException)
    {
        this.argName = argName;
        innerText = text;
    }
}