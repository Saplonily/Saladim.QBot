using System.Text.Json.Serialization;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public class ForwardEntityModel : IEnumerable<ForwardEntityNodeModel>
{
    protected internal IEnumerable<ForwardEntityNodeModel> nodeModels;

    public ForwardEntityModel(ForwardEntity entity)
    {
        var messages = entity.Nodes.Value;
        nodeModels =
            from msg in messages
            let node = msg.AsCast<ForwardContentNode>() ??
            throw new InvalidOperationException("Only content forward node is supported ToModel.")
            select new ForwardEntityNodeModel(
                node.SenderShowName,
                node.Sender.UserId,
                node.MessageEntity.Chain.ToModel(),
                DateTimeHelper.ToUnix(node.SendTime)
                );
    }

    public IEnumerator<ForwardEntityNodeModel> GetEnumerator()
    {
        return nodeModels.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return ((System.Collections.IEnumerable)nodeModels).GetEnumerator();
    }
}

public class ForwardEntityNodeModel
{
    [Ignore(Condition = JsonIgnoreCondition.Never), Name("type")]
    public string Type => "node";

    [Name("data")]
    public Dictionary<string, object> DataDic { get; set; }

    public ForwardEntityNodeModel(string senderName, long userId, CqMessageChainModel chainModel, long time)
    {
        DataDic = new()
        {
            ["name"] = senderName,
            ["uin"] = userId,
            ["content"] = chainModel,
            ["time"] = time
        };
    }
}