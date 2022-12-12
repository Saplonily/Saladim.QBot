using System.Diagnostics;
using System.Text;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay}")]
public class MessageChain : CqEntity, IMessageChain
{
    public List<MessageChainNode> MessageChainNodes { get; protected set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IReadOnlyList<IMessageChainNode> IMessageChain.ChainNodes => MessageChainNodes;

    public MessageChain(CqClient client, IEnumerable<MessageChainNode> nodes) : this(client)
    {
        MessageChainNodes.AddRange(nodes);
    }

    public MessageChain(CqClient client) : base(client)
    {
        MessageChainNodes = new();
    }

    internal static MessageChain FromModel(CqClient client, CqMessageChainModel model)
    {
        List<MessageChainNode> nodes = new();
        foreach (var nodeModel in model.ChainNodeModels)
        {
            var node = MessageChainNode.FromModel(client, nodeModel);
            if (node is not null)
                nodes.Add(node);
        }
        return new MessageChain(client, nodes);
    }

    internal CqMessageChainModel ToModel()
    {
        CqMessageChainModel chainModel = new();
        foreach (var node in this.MessageChainNodes)
        {
            chainModel.ChainNodeModels.Add(node.ToModel());
        }
        return chainModel;
    }

    public T First<T>() where T : MessageChainNode
        => (T)this.MessageChainNodes.First(n => n is T);

    public T? FirstOrNull<T>() where T : MessageChainNode
    => (T?)this.MessageChainNodes.FirstOrDefault(n => n is T);

    public IEnumerable<T> AllOf<T>() where T : MessageChainNode
        => this.MessageChainNodes.Where(n => n is T).Select(n => (T)n);

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

    public IEnumerable<MessageChainAtNode> AllAt()
        => this.AllOf<MessageChainAtNode>();

    public IEnumerable<MessageChainImageNode> AllImage()
        => this.AllOf<MessageChainImageNode>();

    public IEnumerable<MessageChainTextNode> AllText()
        => this.AllOf<MessageChainTextNode>();

    public bool Mentioned(User user)
        => this.AllAt().Where(n => n.User is not null && n.User == user).Select(n => n).Any();

    /// <summary>
    /// <para>是否该消息链中包含回复该消息</para>
    /// <para>
    /// 警告: 因为go-cqhttp的bug: 使用消息id获取的消息链中不包含reply节点,
    /// 所以请勿对 使用消息id获取的消息实体 / 使用SendMessageAsync函数发送消息后的返回值使用该判断函数
    /// 这会导致该函数返回false
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool Replied(Message message)
    {
        var n = this.FirstReplyOrNull();
        if (n is null) return false;
        if (n.MessageBeReplied == message) return true;
        return false;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            StringBuilder sb = new();
            foreach (var node in MessageChainNodes)
            {
                if (node.NodeType is CqCodeType.Text)
                {
                    sb.Append(node.Cast<MessageChainTextNode>().Text);
                }
                else
                {
                    sb.Append('[');
                    sb.Append(node.NodeType);
                    sb.Append(']');
                }
            }
            return sb.ToString();
        }
    }
}
