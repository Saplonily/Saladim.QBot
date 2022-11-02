﻿using System.Diagnostics;

namespace QBotDotnet.GoCqHttp;

[DebuggerDisplay("{Nickname,nq}({Card,nq},{UserId,nq})")]
public class CqGroupMessageSender : CqMessageSender
{
    [Name("card")]
    public string Card { get; set; } = default!;

    [Name("area")]
    public string Area { get; set; } = default!;

    [Name("level")]
    public string Level { get; set; } = default!;

    [Name("role")]
    public string Role { get; set; } = default!;

    [Name("title")]
    public string Title { get; set; } = default!;

}