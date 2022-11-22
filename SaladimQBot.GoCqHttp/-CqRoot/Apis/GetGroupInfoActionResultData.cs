namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupInfoActionResultData : CqApiCallResultData
{
    [Name("group_id")]
    public Int64 GroupId { get; set; }

    [Name("group_name")]
    public string GroupName { get; set; } = default!;

    [Name("group_memo")]
    public string GroupMemo { get; set; } = default!;

    [Name("group_create_time")]
    public UInt32 GroupCreateTime { get; set; }

    [Name("group_level")]
    public UInt32 GroupLevel { get; set; }

    [Name("member_count")]
    public Int32 MemberCount { get; set; }

    [Name("max_member_count")]
    public Int32 MaxMemberCount { get; set; }
}