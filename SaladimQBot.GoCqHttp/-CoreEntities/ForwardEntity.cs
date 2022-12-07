using System.Collections;
using SaladimQBot.Core;

namespace SaladimQBot.GoCqHttp;


public class ForwardEntity : CqEntity, IForwardEntity
{
    protected internal Expirable<List<ForwardNode>> nodes;

    public ForwardEntity(CqClient client, IEnumerable<ForwardNode> nodes) : base(client)
    {
        this.nodes = client.MakeNoneExpirableExpirable(() =>
        {
            List<ForwardNode> forwardNodes = new();
            forwardNodes.AddRange(nodes);
            return forwardNodes;
        });
    }

    IForwardNode IReadOnlyList<IForwardNode>.this[int index] => nodes.Value[index];

    int IReadOnlyCollection<IForwardNode>.Count => nodes.Value.Count;

    IEnumerator<IForwardNode> IEnumerable<IForwardNode>.GetEnumerator() => nodes.Value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => nodes.Value.GetEnumerator();
}
