using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 消息实体
/// </summary>
public class MessageEntity : IMessageEntity
{
    protected internal readonly CqMessageEntity cqEntity;

    public CqMessageEntity CqEntity { get => cqEntity; }

    public string RawString { get => rawString.Value; }


    protected Lazy<string> rawString;

    public MessageEntity(in CqMessageEntity cqEntity, string rawString)
    {
        this.cqEntity = cqEntity;
#if NETSTANDARD2_0
        this.rawString = new Lazy<string>(() => rawString, isThreadSafe: true);
#elif NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        this.rawString = new Lazy<string>(rawString);
#endif
    }

    public MessageEntity(in IMessageEntity entity)
    {
        cqEntity = CqMessageEntity.FromIMessageEntity(entity);
        rawString = new Lazy<string>(() => MessageEntityHelper.CqEntity2RawString(cqEntity), isThreadSafe: true);
    }

    public MessageEntity(CqMessageEntity cqEntity)
    {
        this.cqEntity = cqEntity;
        this.rawString = new Lazy<string>(() => MessageEntityHelper.CqEntity2RawString(cqEntity), isThreadSafe: true);
    }

    #region IMessageEntity

    public int Count => cqEntity.Count;

    IEnumerator<IMessageEntityNode> IEnumerable<IMessageEntityNode>.GetEnumerator()
        => cqEntity.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => cqEntity.GetEnumerator();

    #endregion
}