using QBotDotnet.GoCqHttp.Posts;

namespace QBotDotnet.GoCqHttp;

internal static class CqTypeMapper
{
    public static Type? FindClassFromCqCodeType(CqCodeType cqCode) => cqCode switch
    {
        CqCodeType.Text => typeof(CqMessageTextNode),
        CqCodeType.At => typeof(CqMessageAtNode),
        CqCodeType.Image => typeof(CqMessageImageReceiveNode),
        CqCodeType.Record => null,
        CqCodeType.Reply => typeof(CqMessageReplyIdNode),
        CqCodeType.Face => typeof(CqMessageFaceNode),
        CqCodeType.Unimplemented or
        _ => typeof(CqMessageUnimplementedNode),
    };

    public static Type? FindClassFromCqNoticeType(CqNoticeType noticeType) => noticeType switch
    {
        CqNoticeType.GroupFileUploaded => typeof(CqGroupFileUploadedNoticePost),
        CqNoticeType.GroupAdminChanged => typeof(CqGroupAdminChangedNoticePost),
        CqNoticeType.GroupMemberDecreased => typeof(CqGroupMemberDecreaseNoticePost),
        CqNoticeType.GroupMemberIncreased => typeof(CqGroupMemberIncreaseNoticePost),
        CqNoticeType.GroupMemberBanned => typeof(CqGroupMemberBannedNoticePost),
        CqNoticeType.GroupMemberCardChanged => typeof(CqGroupMemberCardChangedNoticePost),
        CqNoticeType.GroupMessageRecalled => typeof(CqGroupMessageRecalledNoticePost),
        CqNoticeType.FriendRecalled => typeof(CqFriendMessageRecalledNoticePost),
        CqNoticeType.FriendAdded => typeof(CqFriendAddedNoticePost),
        CqNoticeType.SystemNotice => typeof(CqNotifyNoticePost),
        CqNoticeType.OfflineFileUploaded => typeof(CqOfflineFileUploadedNoticePost),
        CqNoticeType.GroupEssenceSet => typeof(CqGroupEssenceSetNoticePost),
        _ => null,
    };

    public static Type? FindClassFromCqMessagePostType(CqMessageSubType messageSubType) => messageSubType switch
    {
        CqMessageSubType.TempFromGroup or
        CqMessageSubType.Friend => typeof(CqPrivateMessagePost),
        CqMessageSubType.Group or
        CqMessageSubType.Anonymous => typeof(CqGroupMessagePost),
        CqMessageSubType.Other => typeof(CqOtherMessagePost),
        CqMessageSubType.GroupFromSelf => throw new NotSupportedException(),
        _ => null
    };
}