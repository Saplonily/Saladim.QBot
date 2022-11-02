using System.Diagnostics;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("{Nickname,nq}({UserId,nq})")]
public class CqMessageSender
{
    [Name("user_id")]
    public Int64 UserId { get; set; }
    [Name("nickname")]
    public string Nickname { get; set; } = default!;
    [Name("sex")]
    public Sex Sex { get; set; }
    [Name("age")]
    public Int32 Age { get; set; }
}