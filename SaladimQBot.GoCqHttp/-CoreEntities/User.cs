using System.Diagnostics;
using System.Net.Sockets;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{Nickname,nq} ({UserId,nq})")]
public class User : CqEntity, IUser
{
    public long UserId { get; protected set; }

    public Expirable<string> Nickname { get; protected set; } = default!;

    public Expirable<Sex> Sex { get; protected set; } = default!;

    public Expirable<int> Age { get; protected set; } = default!;

    public Expirable<string> Qid { get; protected set; } = default!;

    public Expirable<int> Level { get; protected set; } = default!;

    public Expirable<int> LoginDays { get; protected set; } = default!;

    public string CqAt => MessageChainModelHelper.CqStringify(new MessageChainAtNode(Client, UserId, Nickname.Value).ToModel());

    protected Expirable<GetStrangerInfoActionResultData> ApiCallResult { get; set; } = default!;

    protected User(CqClient client, long userId) : base(client)
    {
        UserId = userId;
    }

    /// <summary>
    /// <para>向一个用户发送一条消息, 可能为临时消息</para>
    /// <para>截止2022-12-12仍存在bug, 请勿调用此方法, 详见 https://github.com/Mrs4s/go-cqhttp/issues/1331</para>
    /// </summary>
    /// <param name="groupId">若为临时消息时显示的群来源</param>
    /// <param name="messageEntity">消息实体</param>
    /// <returns>消息实体</returns>
    [Obsolete("请勿使用此方法, 可能会导致账号冻结, 具体消息请见 SaladimQBot.GoCqHttp.User的方法 SendMessageAsync的xml注释")]
    public Task<PrivateMessage> SendMessageAsync(long? groupId, MessageEntity messageEntity)
        => Client.SendPrivateMessageAsync(UserId, groupId, messageEntity);

    /// <summary>
    /// <para>向一个用户发送一条消息, 可能为临时消息</para>
    /// <para>截止2022-12-12仍存在bug, 请勿调用此方法, 详见 https://github.com/Mrs4s/go-cqhttp/issues/1331</para>
    /// </summary>
    /// <param name="groupId">若为临时消息时显示的群来源</param>
    /// <param name="rawString">消息文本</param>
    /// <returns>消息实体</returns>
    [Obsolete("请勿使用此方法, 可能会导致账号冻结, 具体消息请见 SaladimQBot.GoCqHttp.User的方法 SendMessageAsync的xml注释")]
    public Task<PrivateMessage> SendMessageAsync(long? groupId, string rawString)
        => Client.SendPrivateMessageAsync(UserId, groupId, rawString);

    /// <summary>
    /// <para>向一个用户发送一条转发消息, 可能为临时消息</para>
    /// <para>截止2022-12-12仍存在bug, 请勿调用此方法, 详见 https://github.com/Mrs4s/go-cqhttp/issues/1331</para>
    /// </summary>
    /// <param name="groupId">若为临时消息时显示的群来源</param>
    /// <param name="rawString">消息文本</param>
    /// <returns>消息实体</returns>
    [Obsolete("请勿使用此方法, 可能会导致账号冻结, 具体消息请见 SaladimQBot.GoCqHttp.User的方法 SendMessageAsync的xml注释")]
    public Task<PrivateMessage> SendMessageAsync(long? groupId, ForwardEntity forwardEntity)
        => throw new InvalidOperationException("Use sendMessageAsync for friend first.");



    #region CreateFrom / LoadFrom集合

    internal static User CreateFromMessagePost(in CqClient client, in CqMessagePost post)
        => new User(client, post.UserId)
                .LoadApiCallResult(post.UserId)
                .LoadFromUserId()
                .LoadFromMessageSender(post.Sender);

    internal static User CreateFromNicknameAndId(in CqClient client, in string nickname, long userId)
        => new User(client, userId)
                .LoadApiCallResult(userId)
                .LoadFromUserId()
                .LoadNickname(nickname);

    internal static User CreateFromId(in CqClient client, long userId)
        => new User(client, userId)
                .LoadApiCallResult(userId)
                .LoadFromUserId();

    protected internal User LoadNickname(in string nickname)
    {
        this.Nickname = Client.MakeDependencyExpirable(ApiCallResult, nickname, d => d.Nickname);
        return this;
    }

    protected internal User LoadFromMessageSender(in CqMessageSender sender)
    {
        var d = ApiCallResult;
        Sex = Client.MakeDependencyExpirable(d, sender.Sex, d => d.Sex);
        Age = Client.MakeDependencyExpirable(d, sender.Age, d => d.Age);
        Nickname = Client.MakeDependencyExpirable(d, sender.Nickname, d => d.Nickname);
        return this;
    }

    protected internal User LoadFromUserId()
    {
        var d = ApiCallResult;
        Sex = Client.MakeDependencyExpirable(d, d => d.Sex);
        Age = Client.MakeDependencyExpirable(d, d => d.Age);
        Nickname = Client.MakeDependencyExpirable(d, d => d.Nickname);
        Qid = Client.MakeDependencyExpirable(d, d => d.Qid);
        Level = Client.MakeDependencyExpirable(d, d => d.Level);
        LoginDays = Client.MakeDependencyExpirable(d, d => d.LoginDays);
        return this;
    }

    protected internal User LoadApiCallResult(in long userId)
    {
        var api = new GetStrangerInfoAction()
        {
            UserId = userId
        };
        ApiCallResult = Client.MakeExpirableApiCallResultData<GetStrangerInfoActionResultData>(api);
        return this;
    }

    #endregion

    #region IUser

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    long IUser.UserId { get => UserId; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IUser.Nickname { get => Nickname.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Core.Sex IUser.Sex { get => Sex.Value.Cast<Core.Sex>(); }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IUser.Age { get => Age.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IUser.Qid { get => Qid.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IUser.Level { get => Level.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IUser.LoginDays { get => LoginDays.Value; }

    [Obsolete]
    async Task<IMessage> IMessageWindow.SendMessageAsync(IMessageEntity messageEntity)
        => await SendMessageAsync(null, new MessageEntity(Client, messageEntity)).ConfigureAwait(false);

    [Obsolete]
    async Task<IMessage> IMessageWindow.SendMessageAsync(string rawString)
        => await SendMessageAsync(null, rawString).ConfigureAwait(false);

    [Obsolete]
    async Task<IMessage> IMessageWindow.SendMessageAsync(IForwardEntity forwardEntity)
        => await SendMessageAsync(null, forwardEntity is ForwardEntity our ? our : throw new InvalidOperationException(StringConsts.NotSameClientError)).ConfigureAwait(false);

    #endregion

    #region equals重写
    public override bool Equals(object? obj)
    {
        return obj is User user &&
               this.UserId == user.UserId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.UserId);
    }

    public static bool operator ==(User left, User right)
    {
        return EqualityComparer<User>.Default.Equals(left, right);
    }

    public static bool operator !=(User left, User right)
    {
        return !(left == right);
    }
    #endregion
}
