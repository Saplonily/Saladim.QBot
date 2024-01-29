using System.Diagnostics;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 消息实体
/// </summary>
[DebuggerDisplay("{rawString.Value}")]
public class MessageEntity : CqEntity, IMessageEntity
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected Lazy<string> rawString;

    public MessageChain Chain { get; protected set; }

    public string RawString { get => rawString.Value; }

    public MessageEntity(CqClient client, in CqMessageChainModel chainModel, string rawString) : base(client)
    {
        this.Chain = MessageChain.FromModel(client, chainModel);
#if NETSTANDARD2_0
        this.rawString = new Lazy<string>(() => rawString, isThreadSafe: true);
#elif NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        this.rawString = new Lazy<string>(rawString);
#endif
    }

    internal MessageEntity(CqClient client, CqMessageChainModel chainModel) : base(client)
    {
        this.Chain = MessageChain.FromModel(client, chainModel);
        this.rawString = new Lazy<string>(() => MessageChainModelHelper.ChainToRawString(chainModel), isThreadSafe: true);
    }

    public MessageEntity(CqClient client, in IMessageEntity entity) : base(client)
    {
        if (entity is MessageEntity thisEntity && ReferenceEquals(entity.Client, this.Client))
        {
            Chain = thisEntity.Chain;
            rawString = thisEntity.rawString;
            return;
        }
        throw new InvalidOperationException("Cannot create a messageEntity with two different client.");
    }

    public MessageEntity(CqClient client, MessageChain chain, string rawString) : this(client, chain.ToModel(), rawString)
    {
    }

    public MessageEntity(CqClient client, MessageChain chain) : this(client, chain.ToModel())
    {
    }

    public static MessageEntity FromText(CqClient client, string text)
        => new(client, new MessageChain(client, new MessageChainNode[] { new MessageChainTextNode(client, text) }));

    #region 重写

    public override bool Equals(object? obj)
    {
        return obj is MessageEntity entity &&
               EqualityComparer<Lazy<string>>.Default.Equals(rawString, entity.rawString);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(rawString);
    }

    public static bool operator ==(MessageEntity? left, MessageEntity? right)
    {
        return EqualityComparer<MessageEntity>.Default.Equals(left!, right!);
    }

    public static bool operator !=(MessageEntity? left, MessageEntity? right)
    {
        return !(left == right);
    }

    #endregion

    #region chain迁移过来的

    public T First<T>() where T : MessageChainNode
        => (T)this.Chain.MessageChainNodes.First(n => n is T);

    public T? FirstOrNull<T>() where T : MessageChainNode
    => (T?)this.Chain.MessageChainNodes.FirstOrDefault(n => n is T);

    public IEnumerable<T> AllOf<T>() where T : MessageChainNode
        => this.Chain.MessageChainNodes.Where(n => n is T).Select(n => (T)n);

    public MessageChainAtNode FirstAt()
        => this.First<MessageChainAtNode>();

    public MessageChainForwardNode FirstForward()
        => this.First<MessageChainForwardNode>();

    public MessageChainImageNode FirstImage()
        => this.First<MessageChainImageNode>();

    public MessageChainReplyNode FirstReply()
        => this.First<MessageChainReplyNode>();

    public MessageChainTextNode FirstText()
        => this.First<MessageChainTextNode>();

    public MessageChainAtNode? FirstAtOrNull()
        => this.FirstOrNull<MessageChainAtNode>();

    public MessageChainForwardNode? FirstForwardOrNull()
        => this.FirstOrNull<MessageChainForwardNode>();

    public MessageChainImageNode? FirstImageOrNull()
        => this.FirstOrNull<MessageChainImageNode>();

    public MessageChainReplyNode? FirstReplyOrNull()
        => this.FirstOrNull<MessageChainReplyNode>();

    public MessageChainTextNode? FirstTextOrNull()
        => this.FirstOrNull<MessageChainTextNode>();

    /// <summary>
    /// 消息的所有@
    /// </summary>
    public IEnumerable<MessageChainAtNode> AllAt()
        => this.AllOf<MessageChainAtNode>();

    public IEnumerable<MessageChainImageNode> AllImage()
        => this.AllOf<MessageChainImageNode>();

    public IEnumerable<MessageChainTextNode> AllText()
        => this.AllOf<MessageChainTextNode>();

    /// <summary>
    /// 消息是否提及某个用户 (不包含@全体成员)
    /// </summary>
    /// <param name="user">目标用户</param>
    public bool Mentioned(User user)
        => this.AllAt().Where(n => !n.IsMentionAllUser && n.User! == user).Any();

    public bool MentionedAllUser()
        => this.AllAt().Where(n => n.IsMentionAllUser).Any();

    /// <summary>
    /// 消息是否@了bot (不包含@全体成员)
    /// </summary>
    public bool MentionedSelf()
        => this.Mentioned(Client.Self);

    /// <summary>
    /// <para>是否该消息中包含回复该消息</para>
    /// <para>
    /// 警告: 因为go-cqhttp的bug: 使用消息id获取的消息链中不包含reply节点,
    /// 所以请勿对 使用消息id获取的消息实体 / 使用SendMessageAsync函数发送消息后的返回值使用该判断函数
    /// 这会导致该函数返回false
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    /// <returns>消息是否被回复</returns>
    public bool Replied(Message message)
    {
        var n = this.FirstReplyOrNull();
        if (n is null) return false;
        if (n.MessageBeReplied == message) return true;
        return false;
    }

    #endregion

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageChain IMessageEntity.Chain => Chain;
}