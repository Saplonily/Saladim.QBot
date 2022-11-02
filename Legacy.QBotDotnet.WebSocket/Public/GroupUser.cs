using QBotDotnet.GoCqHttp.Internal;

namespace QBotDotnet.Public;

public class GroupUser : User
{
    public long GroupId { get; internal set; }
    public Group GroupIn { get => GroupInLazy.Value; }

    internal Lazy<Group> GroupInLazy = null!;
    public string Card { get; internal set; } = string.Empty;
    public string Area { get; internal set; } = string.Empty;
    public string Title { get; internal set; } = string.Empty;
    public GroupRole Role { get; internal set; } = GroupRole.Invalid;
    internal protected GroupUser(Client client) : base(client) { }

    internal static GroupUser GetFrom(CQGroupMessageSender sender, Client client)
    {
        GroupUser u = new(client)
        {
            Card = sender.Card,
            Area = sender.Area,
            Title = sender.Title,
            Role = sender.Role,
            GroupId = sender.GroupId
        };
        u.GroupInLazy = new Lazy<Group>(() => Group.GetFromId(u.GroupId, client).GetAwaiter().GetResult());
        return u;
    }

}