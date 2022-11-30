namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupMemberListAction : CqCacheableApi
{
    public override string ApiName => "get_group_member_list";

    public override Type? ApiResultDataType => typeof(GetGroupMemberListActionResultData);

    [Name("group_id")]
    public long GroupId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is GetGroupMemberListAction action &&
               base.Equals(obj) &&
               GroupId == action.GroupId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GroupId);
    }
}
