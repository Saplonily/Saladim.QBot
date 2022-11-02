using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("[Image:{Type,nq},{SubType,nq}]")]
[JsonConverter(typeof(CqMessageImageReceiveNodeJsonConverter))]
public class CqMessageImageReceiveNode : CqMessageEntityNode, IMessageImageReceiveNode
{
    public const string FilePropertyName = "file";
    public const string SubTypePropertyName = "subType";
    public const string UrlPropertyName = "url";
    public const string ShowTypePropertyName = "id";
    public const string TypePropertyName = "type";
    public override MessageNodeType NodeType { get => MessageNodeType.Image; }

    public string FileName { get; }
    public string ImageUrl { get; }

    public Core.ImageSendType Type { get; }

    public Core.ImageSendSubType SubType { get; }

    public Core.ImageShowType ShowType { get; }

    internal CqMessageImageReceiveNode(
        string imageUrl,
        string fileName,
        Core.ImageSendType type,
        Core.ImageSendSubType subType,
        Core.ImageShowType showType
        ) =>
        (ImageUrl, Type, SubType, ShowType, FileName) =
        (imageUrl, type, subType, showType, fileName);
}