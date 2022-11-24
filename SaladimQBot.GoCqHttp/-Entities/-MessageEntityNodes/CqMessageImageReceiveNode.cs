using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

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

    public string File { get; }

    public string ImageUrl { get; }

    public ImageSendType Type { get; }

    public ImageSendSubType SubType { get; }

    public ImageShowType ShowType { get; }

    internal CqMessageImageReceiveNode(
        string imageUrl, string fileName,
        ImageSendType type, ImageSendSubType subType, ImageShowType showType
        )
        =>
        (ImageUrl, Type, SubType, ShowType, File) =
        (imageUrl, type, subType, showType, fileName);

    public override IDictionary<string, string> GetParamsDictionary()
    {
        StringDictionary dic = new()
        {
            [UrlPropertyName] = ImageUrl,
            [FilePropertyName] = File
        };
        switch (Type)
        {
            case ImageSendType.Show:
                dic.Add(ShowTypePropertyName, EnumAttributeCacher.GetIntAttrFromEnum(ShowType).ToString());
                dic.Add(TypePropertyName, EnumAttributeCacher.GetStrAttrFromEnum(Type));
                break;
            case ImageSendType.Flash:
                dic.Add(TypePropertyName, EnumAttributeCacher.GetStrAttrFromEnum(Type));
                break;
        }
        if (ShowType is not ImageShowType.Invalid)
        {
            dic.Add(SubTypePropertyName, EnumAttributeCacher.GetIntAttrFromEnum(SubType).ToString());
        }
        return dic;
    }

    IMessageImageSendNode IMessageImageReceiveNode.ToSendNode()
        => ToSendNode();

    public CqMessageImageSendNode ToSendNode()
    {
        return new(this.File);
    }

    #region IMessageImageReceiveNode

    Core.ImageSendType IMessageImageReceiveNode.Type => Type.Cast<Core.ImageSendType>();

    Core.ImageSendSubType IMessageImageReceiveNode.SubType => SubType.Cast<Core.ImageSendSubType>();

    Core.ImageShowType IMessageImageReceiveNode.ShowType => ShowType.Cast<Core.ImageShowType>();

    #endregion
}