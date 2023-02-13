using System;
using System.Text;
using SaladimQBot.Core;

namespace SaladimQBot.Core;

public partial class GenericUniNode : UniNode
{
    protected string name;
    protected IDictionary<string, string> nodeArgs;
    protected string? primaryValue;

    public override string Name => name;

    public override string? PrimaryKey => null;

    public override string? PrimaryValue { get => primaryValue; }

    public GenericUniNode(IClient client, string name, IDictionary<string, string> nodeArgs) : base(client)
    {
        this.name = name;
        this.nodeArgs = nodeArgs;
    }

    public GenericUniNode(IClient client, string name, IDictionary<string, string> nodeArgs, string primaryValue)
        : this(client, name, nodeArgs)
    {
        this.primaryValue = primaryValue;
    }


    public override IDictionary<string, string> Deconstruct() => nodeArgs;

    public override string ToFormattedText()
    {
        StringBuilder sb = new(25);
        sb.Append('<');
        sb.Append(Name);
        if (PrimaryValue is not null)
        {
            sb.Append(':');
            sb.Append(Escape(PrimaryValue));
        }
        if (nodeArgs.Count != 0)
        {
            foreach (var nodeArg in nodeArgs)
            {
                sb.Append(',');
                sb.Append(Escape(nodeArg.Key));
                sb.Append('=');
                sb.Append(Escape(nodeArg.Value));
            }
        }
        sb.Append('>');
        return sb.ToString();
    }
}
