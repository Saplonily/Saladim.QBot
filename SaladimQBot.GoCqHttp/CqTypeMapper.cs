using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

internal static class CqTypeMapper
{
    #region 上报五类型

    public static Type? FindClassForPostType(CqPostType postType) => postType switch
    {
        CqPostType.MessageSent or
        CqPostType.Message => typeof(CqMessagePost),
        CqPostType.Request => null,
        CqPostType.Notice => typeof(CqNoticePost),
        CqPostType.MetaEvent => typeof(CqMetaEventPost),
        _ => null
    };

    public static Type? FindClassForCqNoticeType(CqNoticeType noticeType) => noticeType switch
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

    public static Type? FindClassForCqMessagePostType(CqMessageSubType messageSubType) => messageSubType switch
    {
        CqMessageSubType.TempFromGroup or
        CqMessageSubType.Friend => typeof(CqPrivateMessagePost),
        CqMessageSubType.Group or
        CqMessageSubType.Anonymous => typeof(CqGroupMessagePost),
        CqMessageSubType.Other => typeof(CqOtherMessagePost),
        CqMessageSubType.GroupFromSelf => typeof(CqGroupMessagePost),
        _ => null
    };

    public static Type? FindClassForCqNotifyNoticePostType(CqNotifySubType notifyType) => notifyType switch
    {
        CqNotifySubType.GroupHonorChanged => typeof(CqGroupHonorChangedNotifyNoticePost),
        CqNotifySubType.Poke => typeof(CqPokeNotifyNoticePost),
        CqNotifySubType.LuckyKing => typeof(CqLuckyKingNotifyNoticePost),
        CqNotifySubType.Title => typeof(CqTitleChangedNotifyNoticePost),
        _ => null
    };

    public static Type? FindClassForCqRequestPostType(CqRequestType requestType) => requestType switch
    {
        CqRequestType.Friend => typeof(CqFriendRequestPost),
        CqRequestType.Group => typeof(CqGroupRequestPost),
        CqRequestType.Invalid or
        _ => null
    };


    #endregion
}