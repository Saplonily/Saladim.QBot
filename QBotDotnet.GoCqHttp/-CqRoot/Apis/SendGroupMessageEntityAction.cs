using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Apis;

public class SendGroupMessageEntityAction : CqApi
{
    public override string ApiName { get => "send_msg"; }

    public override Type ApiResultDataType { get => typeof(SendMessageActionResultData); }

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("message")]
    public CqMessageEntity Message { get; set; } = default!;

    [Name("auto_escape")]
    public bool AutoEscape { get => !AsCqCodeString; set => AsCqCodeString = value!; }

    [Ignore]
    public bool AsCqCodeString { get; set; } = false;
}