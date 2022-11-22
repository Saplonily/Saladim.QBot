using System.Diagnostics;
using System.Reflection;
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

    protected Expirable<GetStrangerInfoActionResultData> ApiCallResult { get; set; } = default!;

    protected User(ICqClient client, long userId) : base(client)
    {
        UserId = userId;
    }

    public async Task<PrivateMessage> SendPrivateMessage(MessageEntity messageEntity)
    {
        SendPrivateMessageEntityAction api = new()
        {
            Message = messageEntity.cqEntity,
            UserId = this.UserId
        };
        var rst = await Client.CallApiWithCheckingAsync(api);

        PrivateMessage msg =
            PrivateMessage.CreateFromMessageId(Client, rst.Data!.Cast<SendMessageActionResultData>().MessageId);
        return msg;
    }

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

    async Task<IPrivateMessage> IUser.SendPrivateMessage(IMessageEntity messageEntity)
        => await SendPrivateMessage(new MessageEntity(messageEntity));

    #endregion
}
