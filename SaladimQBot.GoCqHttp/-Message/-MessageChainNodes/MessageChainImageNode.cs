using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainImageNode : MessageChainNode, IMessageChainImageNode
{
    public override CqCodeType NodeType => CqCodeType.Image;

    public Uri FileUri { get; set; }

    public MessageChainImageNode(CqClient client, Uri fileUri) : base(client)
    {
        FileUri = fileUri;
    }

    public MessageChainImageNode(CqClient client, string file, string? url) : base(client)
    {
        FileUri = url is null ? (new(file)) : (new(url));
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["file"] = FileUri.ToString()
        };
        return new(NodeType, dic);
    }
}
