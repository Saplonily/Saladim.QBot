using QBotDotnet.GoCqHttp.Internal;
using QBotDotnet.Misc;

namespace QBotDotnet.Public;

public class Group
{
    public long GroupId { get; protected set; }
    public string GroupName { get; protected set; } = null!;
    public string GroupMemo { get; protected set; } = null!;
    public DateTime GroupCreateTime { get => DateTimeHelper.GetFromUnix(groupCreateTime); }
    public uint GroupLevel { get; protected set; }
    public int MemberCount { get; protected set; }
    public int MaxMemberCount { get; protected set; }
    protected uint groupCreateTime = 0;
    protected Group() { }
    internal static async Task<Group> GetFromId(long groupId, Client client)
    {
        var result = (await client.ApiCaller.SendAsync(new CQGetGroupInfoAction(groupId))).Data;
        return new Group()
        {
            GroupId = groupId,
            GroupName = result.String("group_name", ""),
            GroupMemo = result.String("group_memo", ""),
            groupCreateTime = result.Uint("group_create_time") ?? 0,
            GroupLevel = result.Uint("group_level") ?? 0,
            MemberCount = result.Int("member_count") ?? 0,
            MaxMemberCount = result.Int("max_member_count") ?? 0
        };
    }
}