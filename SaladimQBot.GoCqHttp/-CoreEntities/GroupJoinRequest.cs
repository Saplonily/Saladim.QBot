using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class GroupJoinRequest : CqEntity, IGroupJoinRequest
{
    public JoinedGroup Group { get; protected set; } = null!;

    public User User { get; protected set; } = null!;

    public string Comment { get; protected set; } = null!;

    public RequestProcessStatus ProcessStatus { get; protected set; }

    internal protected string Flag { get; set; } = null!;

    public static GroupJoinRequest CreateFromPost(CqClient client, CqGroupRequestPost post) => new(client)
    {
        Group = JoinedGroup.CreateFromGroupId(client, post.GroupId),
        User = User.CreateFromId(client, post.UserId),
        Comment = post.Comment,
        Flag = post.Flag
    };

    protected GroupJoinRequest(CqClient client) : base(client)
    {
        ProcessStatus = RequestProcessStatus.Idle;
    }

    public async Task<GroupUser> ApproveAsync()
    {
        lock (this)
        {
            MakeSureStatusIsIdle();
            ProcessStatus = RequestProcessStatus.Approved;
        }
        SetGroupAddRequestAction api = new()
        {
            Flag = this.Flag,
            IsApprove = true,
            SubType = CqGroupRequestPost.RequestSubType.Add
        };
        await Client.CallApiWithCheckingAsync(api).ConfigureAwait(false);
        return GroupUser.CreateFromGroupIdAndUserId(this.Client, Group.GroupId, User.UserId);
    }

    public async Task DisapproveAsync(string? reason)
    {
        lock (this)
        {
            MakeSureStatusIsIdle();
            ProcessStatus = RequestProcessStatus.Disapproved;
        }
        SetGroupAddRequestAction api = new()
        {
            Flag = this.Flag,
            IsApprove = false,
            SubType = CqGroupRequestPost.RequestSubType.Add,
            Reason = reason

        };
        await Client.CallApiWithCheckingAsync(api).ConfigureAwait(false);
        return;
    }

    protected void MakeSureStatusIsIdle()
    {
        if (ProcessStatus is not RequestProcessStatus.Idle)
        {
            string text = ProcessStatus is RequestProcessStatus.Approved ?
                "This request has been approved." :
                "This request has been disapproved.";
            throw new InvalidOperationException(text);
        }
    }

    IJoinedGroup IGroupJoinRequest.Group => Group;

    IUser IGroupJoinRequest.User => User;

    async Task<IGroupUser> IGroupJoinRequest.ApproveAsync()
        => await ApproveAsync().ConfigureAwait(false);
}
