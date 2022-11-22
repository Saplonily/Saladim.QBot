namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupInfoAction : CqCacheableApi
{
    public override string ApiName { get => "get_group_info"; }

    public override Type ApiResultDataType { get => typeof(GetGroupInfoActionResultData); }

    [Name("group_id")]
    public long GroupId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GetGroupInfoAction action &&
               base.Equals(obj) &&
               GroupId == action.GroupId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GroupId);
    }
}