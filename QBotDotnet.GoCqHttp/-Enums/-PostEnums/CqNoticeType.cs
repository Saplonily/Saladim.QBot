using System.Text.Json.Serialization;

namespace QBotDotnet.GoCqHttp;

[JsonConverter(typeof(CqEnumJsonConverter))]

public enum CqNoticeType
{
    Invalid,
    [NameIn("group_upload")]
    GroupFileUploaded,
    [NameIn("group_admin")]
    GroupAdminChanged,
    [NameIn("group_decrease")]
    GroupMemberDecreased,
    [NameIn("group_increase")]
    GroupMemberIncreased,
    [NameIn("group_ban")]
    GroupMemberBanned,
    [NameIn("group_card")]
    GroupMemberCardChanged,
    [NameIn("group_recall")]
    GroupMessageRecalled,
    [NameIn("friend_add")]
    FriendAdded,
    [NameIn("friend_recall")]
    FriendRecalled,
    [NameIn("offline_file")]
    OfflineFileUploaded,
    [NameIn("client_status")]
    ClientStatusChanged,
    [NameIn("essence")]
    GroupEssenceSet,
    [NameIn("notify")]
    SystemNotice
}