﻿using System.Text.Json.Serialization;

namespace SaladimQBot.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]
public enum CqPostType
{
    Invalid,
    [NameIn("message")]
    Message,
    [NameIn("request")]
    Request,
    [NameIn("notice")]
    Notice,
    [NameIn("meta_event")]
    MetaEvent,
    [NameIn("message_sent")]
    MessageSent
}