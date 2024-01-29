namespace SaladimQBot.GoCqHttp.Apis;

public class GetMessageAction : CqApi
{
    public override string ApiName => "get_msg";

    public override Type? ApiResultDataType => typeof(GetMessageActionResultData);

    [Name("message_id")]
    public long MessageId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GetMessageAction action &&
               this.MessageId == action.MessageId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.MessageId);
    }
}