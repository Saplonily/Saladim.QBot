namespace QBotDotnet.GoCqHttp.Apis;

public class SendPrivateMessageAction : CqApi
{
    public override string ApiName { get => "send_private_msg"; }
    public override Type ApiResultDataType { get => typeof(SendMessageActionResultData); }

}
