using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class GroupInviteRequest : CqEntity, IGroupInviteRequest
{
    public Group Group { get; protected set; } = null!;

    public User User { get; protected set; } = null!;

    public RequestProcessStatus ProcessStatus { get; protected set; }

    protected internal string Flag { get; set; } = null!;


    public static GroupInviteRequest CreateFromPost(CqClient client, CqGroupRequestPost post) => new(client)
    {
        Group = JoinedGroup.CreateFromGroupId(client, post.GroupId),
        User = User.CreateFromId(client, post.UserId),
        Flag = post.Flag
    };

    public GroupInviteRequest(CqClient client) : base(client)
    {
        ProcessStatus = RequestProcessStatus.Idle;
    }


    public async Task ApproveAsync()
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
            SubType = CqGroupRequestPost.RequestSubType.Invite
        };
        await Client.CallApiWithCheckingAsync(api).ConfigureAwait(false);
    }

    public async Task DisapproveAsync()
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
            SubType = CqGroupRequestPost.RequestSubType.Invite,

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

    IGroup IGroupInviteRequest.Group => Group;

    IUser IGroupInviteRequest.User => User;
}
