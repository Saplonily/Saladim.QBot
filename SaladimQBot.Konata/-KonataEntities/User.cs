using System;
using System.Threading.Tasks;
using Konata.Core;
using SaladimQBot.Core;

namespace SaladimQBot.Konata;

public abstract class User : KqEntity, IUser
{
    internal const string RawUserNotSupported = "Raw user does not support IMessageWindow.";

    public long UserId { get; }

    public string Nickname { get; }

    public abstract Sex Sex { get; }

    public abstract int Age { get; }

    public string Qid => throw new NotSupportedException("Konata does not support getting Qid.");

    public abstract int Level { get; }

    public abstract int LoginDays { get; }

    public Uri AvatarUrl => new($"https://q1.qlogo.cn/g?b=qq&nk={UserId}&s=640");

    protected internal User(KqClient client, long userId, string nickname) : base(client)
    {
        this.UserId = userId;
        this.Nickname = nickname;
    }

    public virtual Task<IMessage> SendMessageAsync(IMessageEntity messageEntity)
        => throw new NotImplementedException(RawUserNotSupported);

    public virtual Task<IMessage> SendMessageAsync(string rawString)
        => throw new NotImplementedException(RawUserNotSupported);

    public virtual Task<IMessage> SendMessageAsync(IForwardEntity forwardEntity)
        => throw new NotImplementedException(RawUserNotSupported);
}
