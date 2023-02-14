using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class PrivateMessage : Message, IPrivateMessage, ICqMessageWindow
{
    public MessageTempSource TempSource { get; protected set; } = default!;

    public long? TempSourceGroupId { get; protected set; } = null;

    public IExpirable<User> ExpPrivateSender { get => ExpSender; }

    public User PrivateSender { get => ExpPrivateSender.Value; }

    public override ICqMessageWindow MessageWindow => this;

    protected internal PrivateMessage(CqClient client, int messageId) : base(client, messageId)
    {
        IsFromGroup = client.MakeNoneExpirableExpirable(false);
    }

    #region load一大堆

    internal static PrivateMessage CreateFromPrivateMessagePost(CqClient client, CqPrivateMessagePost post)
        => new PrivateMessage(client, post.MessageId)
            .LoadFromPrivateMessagePost(post);

    internal static new PrivateMessage CreateFromMessageId(CqClient client, int messageId)
        => new PrivateMessage(client, messageId)
                .LoadGetMessageApiResult()
                .LoadFromMessageId().Cast<PrivateMessage>()
                .LoadMessageTempSourceFromId();

    internal PrivateMessage LoadFromPrivateMessagePost(CqPrivateMessagePost post)
    {
        base.LoadFromMessagePost(post);
        TempSource = post.TempSource;
        if (post.Sender is CqGroupTempMessageSender ts)
        {
            TempSourceGroupId = ts.GroupId;
        }
        return this;
    }

    internal PrivateMessage LoadMessageTempSourceFromId()
    {
        TempSource = MessageTempSource.Invalid;
        return this;
    }

    #endregion

    #region IPrivateMessage & ICqMessageWindow

    Core.MessageTempSource IPrivateMessage.TempSource => TempSource.Cast<Core.MessageTempSource>();

    [Obsolete]
    async Task<Message> ICqMessageWindow.SendMessageAsync(MessageEntity messageEntity)
       => await Sender.SendMessageAsync(TempSourceGroupId, messageEntity).ConfigureAwait(false);

    [Obsolete]
    async Task<Message> ICqMessageWindow.SendMessageAsync(string rawString)
       => await Sender.SendMessageAsync(TempSourceGroupId, rawString).ConfigureAwait(false);

    [Obsolete]
    async Task<Message> ICqMessageWindow.SendMessageAsync(ForwardEntity forwardEntity)
       => await Sender.SendMessageAsync(TempSourceGroupId, forwardEntity).ConfigureAwait(false);

    async Task<IMessage> IMessageWindow.SendMessageAsync(IMessageEntity messageEntity)
        => await Sender.SendMessageAsync(TempSourceGroupId, messageEntity).ConfigureAwait(false);

    [Obsolete]
    async Task<IMessage> IMessageWindow.SendMessageAsync(string rawString)
        => await Sender.SendMessageAsync(TempSourceGroupId, rawString).ConfigureAwait(false);


    Task<IMessage> IMessageWindow.SendMessageAsync(IForwardEntity forwardEntity)
        => throw new InvalidOperationException("Send forward entity to non-friend user is not impl.");


    #endregion
}


