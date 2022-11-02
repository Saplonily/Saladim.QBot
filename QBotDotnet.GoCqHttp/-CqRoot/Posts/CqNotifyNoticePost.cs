using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp.Posts;

public abstract class CqNotifyNoticePost : CqNoticePost
{
    [Name("sub_type")]
    public CqNotifySubType SubType { get; set; }
}