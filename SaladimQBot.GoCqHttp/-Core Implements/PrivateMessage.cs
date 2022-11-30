using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class PrivateMessage : Message, IPrivateMessage
{
    public MessageTempSource TempSource { get; protected set; } = default!;

    public Expirable<User> ExpPrivateSender { get => ExpSender; }

    public override ICqMessageWindow MessageWindow => PrivateSender;

    public User PrivateSender { get => ExpPrivateSender.Value; }

    protected internal PrivateMessage(ICqClient client, int messageId) : base(client, messageId)
    {
    }

    internal static PrivateMessage CreateFromPrivateMessagePost(ICqClient client, CqPrivateMessagePost post)
        => new PrivateMessage(client, post.MessageId)
            .LoadFromPrivateMessagePost(post);

    internal static new PrivateMessage CreateFromMessageId(ICqClient client, int messageId)
        => new PrivateMessage(client, messageId)
                .LoadGetMessageApiResult()
                .LoadFromMessageId().Cast<PrivateMessage>()
                .LoadMessageTempSourceFromId();

    internal PrivateMessage LoadFromPrivateMessagePost(CqPrivateMessagePost post)
    {
        base.LoadFromMessagePost(post);
        TempSource = post.TempSource;
        return this;
    }

    internal PrivateMessage LoadMessageTempSourceFromId()
    {
        TempSource = MessageTempSource.Invalid;
        return this;
    }

    #region IPrivateMessage

    Core.MessageTempSource IPrivateMessage.TempSource => TempSource.Cast<Core.MessageTempSource>();

    #endregion
}


