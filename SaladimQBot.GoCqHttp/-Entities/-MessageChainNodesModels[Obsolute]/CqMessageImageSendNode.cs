using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class CqMessageImageSendNode : CqMessageChainNodeModel
{
    public override MessageNodeType NodeType { get => MessageNodeType.Image; }

    [Name("file")]
    public string File { get; set; } = default!;

    public CqMessageImageSendNode(string file)
    {
        this.File = file;
    }
}