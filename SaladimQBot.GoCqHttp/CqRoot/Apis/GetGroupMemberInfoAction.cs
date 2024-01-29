namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupMemberInfoAction : CqCacheableApi
{
    public override string ApiName => "get_group_member_info";

    public override Type? ApiResultDataType => typeof(GetGroupMemberInfoActionResultData);

    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("user_id")]
    public Int64 UserId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GetGroupMemberInfoAction action &&
               base.Equals(obj) &&
               GroupId == action.GroupId &&
               UserId == action.UserId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GroupId, UserId);
    }
}
