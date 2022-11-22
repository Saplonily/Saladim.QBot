﻿using System.Diagnostics;
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

    public string FileName { get; }
    public string ImageUrl { get; }

    public ImageSendType Type { get; }

    public ImageSendSubType SubType { get; }

    public ImageShowType ShowType { get; }

    Core.ImageSendType IMessageImageReceiveNode.Type => Type.Cast<Core.ImageSendType>();

    Core.ImageSendSubType IMessageImageReceiveNode.SubType => SubType.Cast<Core.ImageSendSubType>();

    Core.ImageShowType IMessageImageReceiveNode.ShowType => ShowType.Cast<Core.ImageShowType>();

    internal CqMessageImageReceiveNode(
        string imageUrl,
        string fileName,
        ImageSendType type,
        ImageSendSubType subType,
        ImageShowType showType)
        =>
        (ImageUrl, Type, SubType, ShowType, FileName) =
        (imageUrl, type, subType, showType, fileName);

    public override string CqStringify()
    {
        StringBuilder sb = new();
        sb.AppendCqHead(this);
        List<string> strs = new(5)
        {
            MessageEntityHelper.GetParaValuePair(UrlPropertyName, ImageUrl),
            MessageEntityHelper.GetParaValuePair(FilePropertyName,FileName)
        };
        switch (Type)
        {
            case ImageSendType.Show:
                strs.Add(MessageEntityHelper.GetParaValuePair(
                    ShowTypePropertyName, EnumAttributeCacher.GetIntAttrFromEnum(ShowType).ToString()
                    ));
                strs.Add(MessageEntityHelper.GetParaValuePair(
                    TypePropertyName, EnumAttributeCacher.GetStrAttrFromEnum(Type)
                    ));
                break;
            case ImageSendType.Flash:
                strs.Add(MessageEntityHelper.GetParaValuePair(
                    TypePropertyName, EnumAttributeCacher.GetStrAttrFromEnum(Type)
                    ));
                break;
        }
        if (ShowType is not ImageShowType.Invalid)
        {
            strs.Add(MessageEntityHelper.GetParaValuePair(
                SubTypePropertyName, EnumAttributeCacher.GetIntAttrFromEnum(SubType).ToString()
                ));
        }
        sb.Append(string.Join(",", strs));
        sb.AppendCqFoot();
        return sb.ToString();
    }
}