using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

public class FriendAddRequest : CqEntity, IFriendAddRequest
{
    public User User { get; protected set; } = default!;

    public string Comment { get; protected set; } = default!;

    public FriendAddRequestProcessStatus ProcessState { get; protected set; }

    protected internal string Flag { get; protected set; } = default!;

    protected FriendAddRequest(CqClient client) : base(client)
    {
        ProcessState = FriendAddRequestProcessStatus.Idle;
    }

    public async Task<FriendUser> ApproveAsync()
    {
        lock (this)
        {
            MakeSureStatusIsIdle();
            ProcessState = FriendAddRequestProcessStatus.Approved;
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
            ProcessState = FriendAddRequestProcessStatus.Disapproved;
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
        if (ProcessState is not FriendAddRequestProcessStatus.Idle)
        {
            string text = ProcessState is FriendAddRequestProcessStatus.Approved ?
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

public enum FriendAddRequestProcessStatus
{
    Idle,
    Approved,
    Disapproved
}