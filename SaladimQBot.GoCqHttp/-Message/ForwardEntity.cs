using System.Collections;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;


public class ForwardEntity : CqEntity, IForwardEntity
{
    public Expirable<List<ForwardNode>> Nodes { get; protected set; }

    internal protected bool IsFromFlag { get; protected set; }

    internal protected string? Flag { get; protected set; }

    protected internal Expirable<GetForwardMessageActionResultData>? ApiCallResultData { get; set; }

    public ForwardEntity(CqClient client, IEnumerable<ForwardNode> nodes) : base(client)
    {
        IsFromFlag = false;
        Nodes = client.MakeNoneExpirableExpirable(() =>
        {
            List<ForwardNode> forwardNodes = new();
            forwardNodes.AddRange(nodes);
            return forwardNodes;
        });
    }

    internal ForwardEntity(CqClient client, string forwardFlag) : base(client)
    {
        IsFromFlag = true;
        Flag = forwardFlag;
        GetForwardMessageAction a = new()
        {
            ForwardId = forwardFlag
        };
        ApiCallResultData = client.MakeExpirableApiCallResultData<GetForwardMessageActionResultData>(a);
        Nodes = client.MakeDependencyExpirable(ApiCallResultData, ForwardNodesFactory);
        List<ForwardNode> ForwardNodesFactory(GetForwardMessageActionResultData d)
        {
            List<ForwardNode> nodes = new();
            foreach (var msg in d.Messages)
            {
                ForwardContentNode n = new(
                    msg.Sender.Nickname,
                    client.GetUser(msg.Sender.UserId),
                    new MessageEntity(Client, msg.Content),
                    DateTimeHelper.GetFromUnix(msg.Time)
                    );
                nodes.Add(n);
            }
            return nodes;
        }
    }

    internal ForwardEntityModel ToModel()
        => new(this);

    IReadOnlyList<IForwardNode> IForwardEntity.ForwardNodes => Nodes.Value;
}
