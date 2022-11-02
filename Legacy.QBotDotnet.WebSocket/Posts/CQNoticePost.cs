using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;
/// <summary>
/// notice上报的基类
/// </summary>
public class CQNoticePost : CQPost
{
    internal CQNoticeType NoticeType { get; private set; } = CQNoticeType.Invalid;

    internal CQNoticePost() { }

    internal new static CQNoticePost GetFrom(JsonElement rootJE, bool doUpdate)
    {
        CQNoticePost post = new();
        post.LoadFrom(rootJE);
        return !doUpdate ? post : post.NoticeType switch
        {
            //TODO: So many notice
            CQNoticeType.GroupUpload => CQGroupUploadNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupAdmin => CQAdminChangeNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupDecrease => CQGroupDecreaseNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupIncrease => CQGroupIncreaseNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupBan => CQGroupBanNoticePost.GetFrom(rootJE),
            CQNoticeType.FriendAdd => CQFriendAddNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupRecall => CQGroupRecallNoticePost.GetFrom(rootJE),
            CQNoticeType.FriendRecall => CQFriendRecallNoticePost.GetFrom(rootJE),
            CQNoticeType.GroupCard => CQGroupCardChangeNoticePost.GetFrom(rootJE),
            CQNoticeType.OfflineFile => CQFriendFileNoticePost.GetFrom(rootJE),
            //CQNoticeType.ClientStatus =>
            //CQNoticeType.Essence =>
            //CQNoticeType.Notify =>
            _ => post,
        };
    }

    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            NoticeType = je.GetProperty("notice_type").GetString() switch
            {
                "group_upload" => CQNoticeType.GroupUpload,
                "group_admin" => CQNoticeType.GroupAdmin,
                "group_decrease" => CQNoticeType.GroupDecrease,
                "group_increase" => CQNoticeType.GroupIncrease,
                "group_ban" => CQNoticeType.GroupBan,
                "friend_add" => CQNoticeType.FriendAdd,
                "group_recall" => CQNoticeType.GroupRecall,
                "friend_recall" => CQNoticeType.FriendRecall,
                "group_card" => CQNoticeType.GroupCard,
                "offline_file" => CQNoticeType.OfflineFile,
                "client_status" => CQNoticeType.ClientStatus,
                "essence" => CQNoticeType.Essence,
                "notify" => CQNoticeType.Notify,
                _ => CQNoticeType.Invalid
            };
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }
    /// <summary>
    /// notice上报类型
    /// </summary>
    public enum CQNoticeType
    {
        Invalid,
        GroupUpload,             // 群文件上传      
        GroupAdmin,              // 群管理员变更      
        GroupDecrease,           // 群成员减少      
        GroupIncrease,           // 群成员增加      
        GroupBan,                // 群成员禁言      
        FriendAdd,               // 好友添加      
        GroupRecall,             // 群消息撤回      
        FriendRecall,            // 好友消息撤回      
        GroupCard,               // 群名片变更      
        OfflineFile,             // 离线文件上传      
        ClientStatus,            // 客户端状态变更      
        Essence,                 // 精华消息      
        Notify                   // 系统通知      
    }
}