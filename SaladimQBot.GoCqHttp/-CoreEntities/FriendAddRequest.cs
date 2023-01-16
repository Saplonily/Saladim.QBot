using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

/// <summary>
/// 好友添加请求实体
/// </summary>
public class FriendAddRequest : CqEntity, IFriendAddRequest
{
    /// <summary>
    /// 请求被添加为好友的用户
    /// </summary>
    public User User { get; protected set; } = default!;

    /// <summary>
    /// 验证消息
    /// </summary>
    public string Comment { get; protected set; } = default!;

    /// <summary>
    /// 该请求的处理情况, 存在此属性是为了方便异步环境下的请求处理
    /// </summary>
    public RequestProcessStatus ProcessState { get; protected set; }

    protected internal string Flag { get; protected set; } = default!;

    protected FriendAddRequest(CqClient client) : base(client)
    {
        ProcessState = RequestProcessStatus.Idle;
    }

    /// <summary>
    /// 同意请求
    /// </summary>
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

    /// <summary>
    /// 拒绝请求
    /// </summary>
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
            IsApprove = false,
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

    internal static FriendAddRequest CreateFromPost(CqClient client, CqFriendRequestPost post) => new(client)
    {
        User = User.CreateFromId(client, post.UserId),
        Comment = post.Comment,
        Flag = post.Flag
    };

    IUser IFriendAddRequest.User => User;

    async Task<IFriendUser> IFriendAddRequest.ApproveAsync()
        => await ApproveAsync().ConfigureAwait(false);
}
