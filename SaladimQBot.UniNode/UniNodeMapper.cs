using SaladimQBot.Core;

namespace SaladimQBot.Core;

public class UniNodeMapper
{
    public static string? NodeTypeToDisplayName(MessageNodeType messageNodeType) => messageNodeType switch
    {
        MessageNodeType.At => "at",
        MessageNodeType.Image => "image",
        MessageNodeType.Record => "record",
        MessageNodeType.Reply => "reply",
        MessageNodeType.Face => "face",
        MessageNodeType.Forward => "forward",
        MessageNodeType.Text or
        MessageNodeType.Invalid or
        MessageNodeType.Unimplemented or
        _ => null
    };
}
