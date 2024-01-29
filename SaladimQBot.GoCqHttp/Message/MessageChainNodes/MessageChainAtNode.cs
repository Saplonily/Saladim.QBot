using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainAtNode : MessageChainNode, IMessageChainAtNode
{
    public override CqCodeType NodeType => CqCodeType.At;

    /// <summary>
    /// 被@的用户, 如果为全体成员则为null
    /// </summary>
    public User? User { get; set; }

    public bool IsMentionAllUser => User is null;

    public string? UserName { get; set; }

    /// <summary>
    /// 使用User实体创建一个At消息节点
    /// </summary>
    /// <param name="client"></param>
    /// <param name="user"></param>
    /// <param name="userName"></param>
    public MessageChainAtNode(CqClient client, User? user, string? userName = null) : base(client)
    {
        User = user;
        UserName = userName;
    }

    public MessageChainAtNode(CqClient client, long userId, string? userName = null)
        : this(client, client.GetUser(userId), userName)
    {
    }

    /// <summary>
    /// 作为提及到的群用户
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <returns>对应群用户, 为@全体成员时为<see langword="null"/></returns>
    public GroupUser? GetMentionedGroupUser(long groupId)
        => User is null ? null : Client.GetGroupUser(groupId, User);

    /// <summary>
    /// 作为提及到的群用户
    /// </summary>
    /// <param name="group">群</param>
    /// <returns>对应群用户, 为@全体成员时为<see langword="null"/></returns>
    public GroupUser? GetMentionedGroupUser(JoinedGroup group)
        => User is null ? null : Client.GetGroupUser(group, User);

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["qq"] = User is null ? "all" : User.UserId.ToString()
        };
        if (UserName is not null)
            dic.Add("name", UserName);
        return new(NodeType, dic);
    }

    IUser? IMessageChainAtNode.User => User;

}
