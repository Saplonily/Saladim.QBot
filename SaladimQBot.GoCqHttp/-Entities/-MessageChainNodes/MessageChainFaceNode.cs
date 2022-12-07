using System.Diagnostics;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("[Face:{FaceId}]")]
public class MessageChainFaceNode : MessageChainNode, IMessageChainFaceNode
{
    public override CqCodeType NodeType => CqCodeType.Face;

    public int FaceId { get; set; }

    public MessageChainFaceNode(CqClient client, int faceId) : base(client)
    {
        FaceId = faceId;
    }

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["id"] = FaceId.ToString()
        };
        return new(NodeType, dic);
    }
}
