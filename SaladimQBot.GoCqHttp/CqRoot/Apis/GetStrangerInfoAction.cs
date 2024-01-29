namespace SaladimQBot.GoCqHttp.Apis;
public class GetStrangerInfoAction : CqCacheableApi
{
    public override string ApiName => "get_stranger_info";

    public override Type? ApiResultDataType => typeof(GetStrangerInfoActionResultData);

    [Name("user_id")]
    public Int64 UserId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GetStrangerInfoAction action &&
               base.Equals(obj) &&
               UserId == action.UserId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), UserId);
    }
}
