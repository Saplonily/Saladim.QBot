using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

/// <summary>
/// Client对象发生的异常
/// </summary>
public class ClientException : Exception
{
    /// <summary>
    /// 发生异常的Client
    /// </summary>
    public IClient Client { get; }
    public ExceptionType Type { get; }

    /// <summary>
    /// 创建一个ClientException对象
    /// </summary>
    /// <param name="client">client</param>
    /// <param name="type"></param>
    /// <param name="message">message</param>
    public ClientException(IClient client, ExceptionType type, string? message = null, Exception? innerException = null)
        : base(message ?? GetDefaultMessage(type), innerException)
    {
        Client = client;
        Type = type;
    }

    internal static string GetDefaultMessage(ExceptionType type) => type switch
    {
        ExceptionType.Invalid =>
            throw new ArgumentException(paramName: nameof(type), message: "no message for invalid type."),
        ExceptionType.AlreadyStarted =>
            "The client has already started.",
        ExceptionType.AlreadyStopped =>
            "The client has already been stopped after started.",
        ExceptionType.NotStartedBefore =>
            "The client hasn't started before stop it.",
        ExceptionType.TimeOut =>
            "The client connected timeout.",
        ExceptionType.WebSocketError =>
            "Internal WebSocket error. " +
            "If you seen this error without exactly error type please raise a issue to us.",
        _ => throw new ArgumentException(paramName: nameof(type), message: "Unknown excpetion type."),
    };

    public enum ExceptionType
    {
        /// <summary>
        /// 无效的，该枚举值不应该出现在实际赋值中
        /// </summary>
        Invalid,
        /// <summary>
        /// Client已经开启
        /// </summary>
        AlreadyStarted,
        /// <summary>
        /// Client已经结束
        /// </summary>
        AlreadyStopped,
        /// <summary>
        /// Client还未开始就尝试进行结束
        /// </summary>
        NotStartedBefore,
        /// <summary>
        /// 连接超时
        /// </summary>
        TimeOut,
        /// <summary>
        /// WebSocket内部错误，具体Exception会放入innerException中，类型为WebSocketException
        /// </summary>
        WebSocketError
    }
}