using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQMessagePost : CQPost
{
    public CQPostMessageSubType MessageSubType { get; private set; } = CQPostMessageSubType.Invalid;
    public int MessageId { get; private set; } = -1;
    public long UserId { get; private set; } = -1;
    public CQMessage Message { get; private set; } = null!;
    public string RawMessage { get; private set; } = string.Empty;
    public int Font { get; private set; } = -1;
    public CQMessageSender Sender { get; private set; } = null!;
    protected CQMessagePost() { }

    internal new static CQMessagePost GetFrom(JsonElement rootJE, bool doUpdate)
    {
        var p = new CQMessagePost();
        p.LoadFrom(rootJE);
        return !doUpdate ? p : p.MessageSubType switch
        {
            CQPostMessageSubType.Group or
            CQPostMessageSubType.GroupSelf or
            CQPostMessageSubType.Anonymous => CQGroupMessagePost.GetFrom(rootJE, doUpdate),

            CQPostMessageSubType.Friend or
            CQPostMessageSubType.TempFromGroup or
            CQPostMessageSubType.GroupSelf => CQPrivateMessagePost.GetFrom(rootJE),
            _ => p
        };
    }

    internal override void LoadFrom(JsonElement rootJE)
    {
        base.LoadFrom(rootJE);
        try
        {
            MessageSubType = rootJE.GetProperty("sub_type").GetString() switch
            {
                "friend" => CQPostMessageSubType.Friend,
                "group" => CQPostMessageSubType.TempFromGroup,
                "group_self" => CQPostMessageSubType.GroupSelf,
                "normal" => CQPostMessageSubType.Group,
                "anonymous" => CQPostMessageSubType.Anonymous,
                "notice" => CQPostMessageSubType.Notice,
                _ => CQPostMessageSubType.Invalid
            };
            MessageId = rootJE.GetProperty("message_id").GetInt32();
            UserId = rootJE.GetProperty("user_id").GetInt64();
            Message = CQMessage.GetFrom(rootJE.GetProperty("message"));
            RawMessage = rootJE.GetProperty("raw_message").GetString() ?? "";
            Font = rootJE.GetProperty("font").GetInt32();
            Sender = CQMessageSender.GetFrom(rootJE, true);
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(rootJE), e);
        }
    }

    public enum CQPostMessageSubType
    {
        Invalid,
        Friend,             //好友
        TempFromGroup,      //来自群的临时会话
        GroupSelf,          //群自己发的消息
        Group,              //普通群消息
        Anonymous,          //群匿名消息
        Notice              //通知
    }
}