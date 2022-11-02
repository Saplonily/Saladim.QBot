using System.Diagnostics;
using System.Text.Json.Serialization;
using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("[{NodeType,nq}:{UserIdStr,nq}]")]
public class CqMessageAtNode : CqMessageEntityNode, IMessageAtNode
{
    [Ignore]
    public override MessageNodeType NodeType { get => (MessageNodeType)CqCodeType.At; }

    [Name("qq")]
    [JsonInclude]
    public string UserIdStr { get; internal set; } = default!;

    [Ignore]
    public long UserId { get => Int64.Parse(UserIdStr); set => UserIdStr = value.ToString(); }

    [Name("name")]
    public string? UserName { get; set; }

    [JsonConstructor]
    public CqMessageAtNode(string userIdStr, string userName)
        => (UserIdStr, UserName) = (userIdStr, userName);


    public CqMessageAtNode(long userId, string userName)
        => (UserId, UserName) = (userId, userName);

}