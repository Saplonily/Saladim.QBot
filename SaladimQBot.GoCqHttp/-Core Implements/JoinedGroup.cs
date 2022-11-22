using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class JoinedGroup : Group, IJoinedGroup
{
    /// <summary>
    /// 群创建时间
    /// </summary>
    public Expirable<DateTime> CreateTime { get; protected set; } = default!;

    /// <summary>
    /// 群活跃等级
    /// </summary>
    public Expirable<uint> GroupLevel { get; protected set; } = default!;

    /// <summary>
    /// 群成员人数
    /// </summary>
    public Expirable<int> MembersCount { get; protected set; } = default!;

    /// <summary>
    /// 群最大成员数
    /// </summary>
    public Expirable<int> MaxMembersCount { get; protected set; } = default!;

    /// <summary>
    /// 群成员
    /// </summary>
    public Expirable<IReadOnlyList<GroupUser>> Members { get; protected set; } = default!;

    protected Expirable<GetGroupMemberListActionResultData> GetMemberListApiResultData { get; set; } = default!;

    protected internal JoinedGroup(ICqClient client, long groupId) : base(client, groupId)
    {
    }

    #region CreateFrom / LoadFrom集合

    internal static new JoinedGroup CreateFromGroupId(ICqClient client, long groupId)
        => new JoinedGroup(client, groupId)
                .LoadFromGroupId(groupId)
                .LoadMemberList(groupId);

    internal static new JoinedGroup CreateFromCqGroupMessagePost(ICqClient client, CqGroupMessagePost post)
        => CreateFromGroupId(client, post.GroupId);

    protected internal new JoinedGroup LoadFromGroupId(long groupId)
    {
        var api = new GetGroupInfoAction() { GroupId = groupId };
        var d = ApiCallResultData = Client.MakeExpirableApiCallResultData<GetGroupInfoActionResultData>(api);
        Name = Client.MakeDependencyExpirable(d, d => d.GroupName);
        Remark = Client.MakeDependencyExpirable(d, d => d.GroupMemo);
        CreateTime = Client.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.GroupCreateTime));
        GroupLevel = Client.MakeDependencyExpirable(d, d => d.GroupLevel);
        MembersCount = Client.MakeDependencyExpirable(d, d => d.MemberCount);
        MaxMembersCount = Client.MakeDependencyExpirable(d, d => d.MaxMemberCount);

        this.LoadMemberList(groupId);
        return this;
    }

    protected internal JoinedGroup LoadMemberList(long groupId)
    {
        var getListApi = new GetGroupMemberListAction() { GroupId = groupId };
        var d = GetMemberListApiResultData =
            Client.MakeExpirableApiCallResultData<GetGroupMemberListActionResultData>(getListApi);
        Members = Client.MakeDependencyExpirable(d, DataToList);
        return this;
        IReadOnlyList<GroupUser> DataToList(GetGroupMemberListActionResultData data)
        {
            var rst = from subData in data.DataList
                      let groupUser = GroupUser.CreateFromGroupIdAndUserId(Client, groupId, subData.UserId)
                      select groupUser;
            return rst.ToList();
        }
    }

    #endregion

    /// <inheritdoc cref="IJoinedGroup.SendMessageAsync(IMessageEntity)"/>
    public async Task<GroupMessage> SendMessageAsync(IMessageEntity messageEntity)
    {
        SendGroupMessageEntityAction a = new()
        {
            GroupId = GroupId,
            Message = CqMessageEntity.FromIMessageEntity(messageEntity)
        };
        var result = (await Client.CallApiWithCheckingAsync(a)).Cast<SendMessageActionResultData>()
        return GroupMessage.CreateFromMessageId(Client, result.MessageId);
    }

    /// <inheritdoc cref="IJoinedGroup.SendMessageAsync(string)"/>
    public async Task<GroupMessage> SendMessageAsync(string message)
    {
        SendGroupMessageAction a = new()
        {
            AsCqCodeString = true,
            GroupId = this.GroupId,
            Message = message
        };
        var result = (await Client.CallApiWithCheckingAsync(a)).Cast<SendMessageActionResultData>();
        return GroupMessage.CreateFromMessageId(Client, result.MessageId);
    }

    #region IJoinedGroup

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IJoinedGroup.CreateTime { get => CreateTime.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    uint IJoinedGroup.GroupLevel { get => GroupLevel.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IJoinedGroup.MembersCount { get => MembersCount.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IJoinedGroup.MaxMembersCount { get => MaxMembersCount.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IEnumerable<IGroupUser> IJoinedGroup.Members { get => Members.Value; }

    async Task<IGroupMessage> IJoinedGroup.SendMessageAsync(IMessageEntity messageEntity)
        => await SendMessageAsync(messageEntity);

    async Task<IGroupMessage> IJoinedGroup.SendMessageAsync(string message)
        => await SendMessageAsync(message);

    #endregion

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Name} ({GroupId})";
}
