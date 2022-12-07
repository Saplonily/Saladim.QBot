using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainImageNode : MessageChainNode, IMessageChainImageNode
{
    public override CqCodeType NodeType => CqCodeType.Image;

    public string FileUri { get; set; }

    public MessageChainImageNode(CqClient client, string file) : base(client)
    {
        FileUri = file;
    }


    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["file"] = FileUri
        };
        return new(NodeType, dic);
    }
}
