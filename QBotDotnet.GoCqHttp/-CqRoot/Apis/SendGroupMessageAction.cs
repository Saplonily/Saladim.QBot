using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Apis;

public class SendGroupMessageAction : CqApi
{
    [Ignore]
    public override string ApiName { get => "send_group_msg"; }

    [Ignore]
    public override Type ApiResultDataType { get => typeof(SendMessageActionResultData); }

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("message")]
    public string Message { get; set; } = default!;

    [Name("auto_escape")]
    public bool AutoEscape { get => !AsCqCodeString; set => AsCqCodeString = value!; }

    [Ignore]
    public bool AsCqCodeString { get; set; } = true;
}