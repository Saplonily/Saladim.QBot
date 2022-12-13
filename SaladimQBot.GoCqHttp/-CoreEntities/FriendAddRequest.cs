using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class FriendAddRequest : CqEntity, IFriendAddRequest
{
    public User User { get; protected set; } = default!;

    public string Comment { get; protected set; } = default!;

    public RequestProcessStatus ProcessState { get; protected set; }

    protected internal string Flag { get; protected set; } = default!;

    protected FriendAddRequest(CqClient client) : base(client)
    {
        ProcessState = RequestProcessStatus.Idle;
    }

    public async Task<FriendUser> ApproveAsync()
    {
        lock (this)
        {
            MakeSureStatusIsIdle();
            ProcessState = RequestProcessStatus.Approved;
        }
        SetFriendAddRequestAction api = new()
        {
            Flag = this.Flag,
            IsApprove = true
        };
        await Client.CallApiWithCheckingAsync(api).ConfigureAwait(false);
        return FriendUser.CreateFromId(this.Client, User.UserId);
    }

    public async Task DisapproveAsync()
    {
        lock (this)
        {
            MakeSureStatusIsIdle();
            ProcessState = RequestProcessStatus.Disapproved;
        }
        SetFriendAddRequestAction api = new()
        {
            Flag = this.Flag,
            IsApprove = false
        };
        await Client.CallApiWithCheckingAsync(api).ConfigureAwait(false);
        return;
    }

    protected void MakeSureStatusIsIdle()
    {
        if (ProcessState is not RequestProcessStatus.Idle)
        {
            string text = ProcessState is RequestProcessStatus.Approved ?
                "This request has been approved." :
                "This request has been disapproved.";
            throw new InvalidOperationException(text);
        }
    }

    public static FriendAddRequest CreateFromPost(CqClient client, CqFriendRequestPost post) => new(client)
    {
        User = User.CreateFromId(client, post.UserId),
        Comment = post.Comment,
        Flag = post.Flag
    };

    IUser IFriendAddRequest.User => User;

    async Task<IFriendUser> IFriendAddRequest.ApproveAsync()
        => await ApproveAsync().ConfigureAwait(false);
}
