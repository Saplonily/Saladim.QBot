using System.Diagnostics;
using System.Text.Json.Serialization;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("[{NodeType,nq}:{FaceIdStr,nq}]")]
public class CqMessageFaceNode : CqMessageEntityNode, IMessageFaceNode
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

    public override string CqStringify()
    {
        return $"[CQ:face,id={FaceIdStr}]";
    }
}