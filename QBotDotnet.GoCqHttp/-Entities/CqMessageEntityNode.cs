using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;


public abstract class CqMessageEntityNode : IMessageEntityNode
{
    [Ignore]
    public abstract MessageNodeType NodeType { get; }
}
//不作为abstract是因为json反序列化器需要一个构造函数
//哈?