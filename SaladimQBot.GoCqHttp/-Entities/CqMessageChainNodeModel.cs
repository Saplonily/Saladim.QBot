using System.Text.Json;
using System.Text.Json.Nodes;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;


internal class CqMessageChainNodeModel
{
    [Ignore]
    public MessageNodeType NodeType { get => CqCodeType.Cast<MessageNodeType>(); }

    [Ignore]
    internal string? RawCqCodeName { get; set; } = default;

    [Name(StringConsts.CqCodeTypeProperty)]
    public CqCodeType CqCodeType { get; }

    [Name(StringConsts.CqCodeParamsProperty)]
    public IDictionary<string, string> Params { get; }

    public CqMessageChainNodeModel(CqCodeType cqCodeType, IDictionary<string, string> @params)
    {
        CqCodeType = cqCodeType is CqCodeType.Invalid ? CqCodeType.Unimplemented : cqCodeType;
        Params = @params;
    }
}