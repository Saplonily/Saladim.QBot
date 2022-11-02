using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Apis;

public class SendGroupMessageAction : CqApi
{
    public override string ApiName { get => "send_msg"; }

    public override Type ApiResultDataType { get => typeof(SendGroupMessageActionResultData); }

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("message")]
    public string Message { get; set; } = default!;

    [Name("auto_escape")]
    public bool AutoEscape { get => !AsCqCodeString; set => AsCqCodeString = value!; }

    [Ignore]
    public bool AsCqCodeString { get; set; } = true;
}