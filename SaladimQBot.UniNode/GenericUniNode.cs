using System;
using System.Text;
using SaladimQBot.Core;

namespace SaladimQBot.Core;

public partial class GenericUniNode : UniNode
{
    protected string name;
    protected IDictionary<string, string> nodeArgs;
    protected string? primaryValue;

    /// <summary>
    /// 名称
    /// </summary>
    public override string Name => name;

    /// <summary>
    /// 主键名称, 实现为恒为<see langword="null"/>
    /// </summary>
    public override string? PrimaryKey => null;

    /// <summary>
    /// 主键值
    /// </summary>
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
        => UniNode.ToFormattedText(Name, primaryValue, nodeArgs);

    internal string GetRequiredArg(string argName, bool tryGetMainKeyAlso = false)
    {
        nodeArgs.TryGetValue(argName, out string? result);
        if (tryGetMainKeyAlso)
        {
            if (PrimaryValue is not null) result = PrimaryValue;
        }
        if(result is null)
        {
            string msg = $"Could not found required node arg \"{argName}\". " +
                $"{nameof(tryGetMainKeyAlso)} is {tryGetMainKeyAlso}";
            throw new KeyNotFoundException(msg);
        }
        return result;
    }
}
