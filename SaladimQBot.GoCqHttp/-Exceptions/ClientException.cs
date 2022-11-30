using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

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
        : base($"{GetMessagePrefix(type)} {message}", innerException)
    {
        Client = client;
        Type = type;
    }

    internal static string GetMessagePrefix(ExceptionType type) => type switch
    {
        ExceptionType.Invalid =>
            throw new ArgumentException(paramName: nameof(type), message: "No message for invalid type."),
        ExceptionType.AlreadyStarted =>
            "The client has already started.",
        ExceptionType.AlreadyStopped =>
            "The client has already been stopped after started.",
        ExceptionType.NotStartedBefore =>
            "The client hasn't started before stop it.",
        ExceptionType.TimeOut =>
            "The client connected timeout.",
        ExceptionType.SessionInternal =>
            "Internal session error. ",
        ExceptionType.NotStartedBeforeCallApi =>
            "The client hasn't started before call an api.",
        ExceptionType.ImplicitApiCallFailed =>
            "Implicit api call failed.",
        ExceptionType.ExplicitApiCallFailed =>
            "Explicit api call failed.",
        ExceptionType.PostParsingFailed =>
            "Post parsing failed, if it isn't caused by a custom post emit, please raise a issue.",
        _ => throw new ArgumentException(paramName: nameof(type), message: "Unknown exception type."),
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
        /// Session内部错误，具体Exception会放入innerException中，类型由session类型决定
        /// </summary>
        SessionInternal,
        /// <summary>
        /// Client还未开始就尝试调用Api
        /// </summary>
        NotStartedBeforeCallApi,
        /// <summary>
        /// 隐式api调用失败(例如获取原本上报不存在字段时会隐式调用获取api)
        /// </summary>
        ImplicitApiCallFailed,
        /// <summary>
        /// 显式api调用失败(例如调用<see cref="IUser"/>的发送私聊消息api)
        /// </summary>
        ExplicitApiCallFailed,
        /// <summary>
        /// 解析 上报/api调用结果 时出现错误,小概率是上报出错
        /// </summary>
        PostParsingFailed
    }
}