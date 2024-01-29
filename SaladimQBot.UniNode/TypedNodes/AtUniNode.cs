using SaladimQBot.Core;

namespace SaladimQBot.Core;

public class AtUniNode : TypedUniNode
{
    public IUser User { get; set; }

    public long UserId => User.UserId;

    public override MessageNodeType Type => MessageNodeType.At;

    public override string PrimaryKey => "id";

    public override string PrimaryValue => User.UserId.ToString();

    public AtUniNode(IClient client, IUser user) : base(client)
    {
        User = user;
    }

    public AtUniNode(IClient client, long groupUserId)
        : this(client, client.GetUser(groupUserId))
    { }

    public AtUniNode(GenericUniNode genericUniNode)
        : this(genericUniNode.Client, long.Parse(genericUniNode.GetRequiredArg("id", true)))
    {
    }


    public override IDictionary<string, string> Deconstruct()
        => new Dictionary<string, string>()
        {
            [PrimaryKey] = PrimaryValue,
            ["id"] = UserId.ToString()
        };
}
