namespace SaladimQBot.GoCqHttp.Apis;

public class SendGroupMessageAction : CqApi
{
    public override string ApiName => "send_group_msg";
    public override Type ApiResultDataType => typeof(SendMessageActionResultData);

    [Name("group_id")]
    public long GroupId { get; set; }

    [Name("message")]
    public string Message { get; set; } = default!;

    [Name("auto_escape")]
    public bool AutoEscape { get => !AsCqCodeString; set => AsCqCodeString = value!; }

    [Ignore]
    public bool AsCqCodeString { get; set; } = true;
}