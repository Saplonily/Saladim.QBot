using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{Nickname,nq} ({UserId,nq})")]
public class User : CqEntity, IUser, ICqMessageWindow
{
    public long UserId { get; protected set; }

    public Expirable<string> Nickname { get; protected set; } = default!;

    public Expirable<Sex> Sex { get; protected set; } = default!;

    public Expirable<int> Age { get; protected set; } = default!;

    public Expirable<string> Qid { get; protected set; } = default!;

    public Expirable<int> Level { get; protected set; } = default!;

    public Expirable<int> LoginDays { get; protected set; } = default!;

    public string CqAt { get => new CqMessageAtNode(this.UserId).CqStringify(); }

    protected Expirable<GetStrangerInfoActionResultData> ApiCallResult { get; set; } = default!;

    protected User(ICqClient client, long userId) : base(client)
    {
        UserId = userId;
    }

    public Task<PrivateMessage> SendMessageAsync(MessageEntity messageEntity)
        => Client.SendPrivateMessageAsync(UserId, messageEntity);

    public Task<PrivateMessage> SendMessageAsync(string rawString)
        => Client.SendPrivateMessageAsync(UserId, rawString);

    #region CreateFrom / LoadFrom集合

    internal static User CreateFromMessagePost(in ICqClient client, in CqMessagePost post)
        => new User(client, post.UserId)
                .LoadApiCallResult(post.UserId)
                .LoadFromUserId()
                .LoadFromMessageSender(post.Sender);

    internal static User CreateFromNicknameAndId(in ICqClient client, in string nickname, long userId)
        => new User(client, userId)
                .LoadApiCallResult(userId)
                .LoadFromUserId()
                .LoadNickname(nickname);

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

    async Task<IMessage> IMessageWindow.SendMessageAsync(IMessageEntity messageEntity)
        => await Client.SendPrivateMessageAsync(UserId, messageEntity);

    async Task<IMessage> IMessageWindow.SendMessageAsync(string rawString)
        => await Client.SendPrivateMessageAsync(UserId, rawString);

    async Task<Message> ICqMessageWindow.SendMessageAsync(MessageEntity messageEntity)
        => await SendMessageAsync(messageEntity);

    async Task<Message> ICqMessageWindow.SendMessageAsync(string rawString)
        => await SendMessageAsync(rawString);

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
