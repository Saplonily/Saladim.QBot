﻿namespace QBotDotnet.GoCqHttp.Posts;

public class CqFriendMessageRecalledNoticePost : CqNoticePost
{
    [Name("user_id")]
    public Int64 UserId { get; set; }

    [Name("message_id")]
    public Int64 MessageId { get; set; }
}