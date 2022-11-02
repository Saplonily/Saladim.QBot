using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Apis;

public class GetGroupInfoActionResultData : CqApiCallResultData
{
    public GetGroupInfoActionResultData()
    {
    }
    public GetGroupInfoActionResultData(CqApiCallResult resultIn) : base(resultIn)
    {
    }

    [Name("group_id")]
    public long GroupId { get; set; } = default!;
    [Name("group_name")]
    public string GroupName { get; set; } = default!;
    [Name("group_memo")]
    public string GroupMemo { get; set; } = default!;
    [Name("group_create_time")]
    public uint GroupCreateTime { get; set; } = default!;
    [Name("group_level")]
    public uint GroupLevel { get; set; } = default!;
    [Name("member_count")]
    public int MemberCount { get; set; } = default!;
    [Name("max_member_count")]
    public int MaxMemberCount { get; set; } = default!;
}