using System.Diagnostics;
using System.Text.Json.Serialization;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("[{NodeType,nq}:{FaceIdStr,nq}]")]
public class CqMessageFaceNode : CqMessageChainNodeModel
{
    [Ignore]
    public override MessageNodeType NodeType { get => (MessageNodeType)CqCodeType.Face; }

    [Ignore]
    public int FaceId { get => int.Parse(FaceIdStr); set => FaceIdStr = value.ToString(); }

    [Name("id")]
    [JsonInclude]
    public string FaceIdStr { get; internal set; } = default!;

    public CqMessageFaceNode(int faceId)
        => FaceId = faceId;

    [JsonConstructor]
    public CqMessageFaceNode(string faceIdStr)
        => FaceIdStr = faceIdStr;
}