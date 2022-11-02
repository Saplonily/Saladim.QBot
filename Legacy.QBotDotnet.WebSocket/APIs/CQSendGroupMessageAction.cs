namespace QBotDotnet.GoCqHttp.Internal;

public class CQSendGroupMessageAction : CQSendMessageAction
{
    public override string ApiName { get => "send_group_msg"; }
    public CQSendGroupMessageAction(long groupId, CQMessage msg)
    {
        GroupId = groupId;
        Message = msg;
    }
}