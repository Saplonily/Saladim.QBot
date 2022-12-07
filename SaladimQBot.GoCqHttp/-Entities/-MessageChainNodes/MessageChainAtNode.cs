using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;

public class MessageChainAtNode : MessageChainNode, IMessageChainAtNode
{
    public override CqCodeType NodeType => CqCodeType.At;

    public User User { get; set; }

    public string? UserName { get; set; }

    public MessageChainAtNode(CqClient client, User user, string? userName = null) : base(client)
    {
        User = user;
        UserName = userName;
    }

    public MessageChainAtNode(CqClient client, long userId, string? userName = null)
        : this(client, client.GetUser(userId), userName)
    {
    }

    public GroupUser GetMentionedGroupUser(long groupId)
        => Client.GetGroupUser(groupId, User);

    public GroupUser GetMentionedGroupUser(JoinedGroup group)
        => Client.GetGroupUser(group, User);

    internal override CqMessageChainNodeModel ToModel()
    {
        StringDictionary dic = new()
        {
            ["qq"] = User.UserId.ToString()
        };
        if (UserName is not null)
            dic.Add("name", UserName);
        return new(NodeType, dic);
    }

    IUser IMessageChainAtNode.User
    {
        get => User;
        set => User = value is User user ?
            user :
            Client.GetUser(value.UserId);
    }
}
