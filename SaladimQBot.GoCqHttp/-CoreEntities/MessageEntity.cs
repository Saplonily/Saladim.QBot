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

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IMessageChain IMessageEntity.Chain => Chain;
}