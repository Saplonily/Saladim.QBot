using System.Collections;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 消息实体
/// </summary>
public class MessageEntity : IMessageEntity
{
    protected internal readonly CqMessageChain cqChainEntity;

    public CqMessageChain Chain { get => cqChainEntity; }

    public string RawString { get => rawString.Value; }


    protected Lazy<string> rawString;

    public MessageEntity(in CqMessageChain cqEntity, string rawString)
    {
        this.cqChainEntity = cqEntity;
#if NETSTANDARD2_0
        this.rawString = new Lazy<string>(() => rawString, isThreadSafe: true);
#elif NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        this.rawString = new Lazy<string>(rawString);
#endif
    }

    public MessageEntity(in IMessageEntity entity)
    {
        cqChainEntity = CqMessageChain.FromIMessageEntity(entity);
        rawString = new Lazy<string>(() => MessageChainHelper.ChainToRawString(cqChainEntity), isThreadSafe: true);
    }

    public MessageEntity(CqMessageChain cqEntity)
    {
        this.cqChainEntity = cqEntity;
        this.rawString = new Lazy<string>(() => MessageChainHelper.ChainToRawString(cqEntity), isThreadSafe: true);
    }

    #region IMessageEntity

    public int Count => cqChainEntity.Count;

    IEnumerator<IMessageEntityNode> IEnumerable<IMessageEntityNode>.GetEnumerator()
        => cqChainEntity.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => cqChainEntity.GetEnumerator();

    #endregion

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
        return EqualityComparer<MessageEntity>.Default.Equals(left, right);
    }

    public static bool operator !=(MessageEntity? left, MessageEntity? right)
    {
        return !(left == right);
    }
}