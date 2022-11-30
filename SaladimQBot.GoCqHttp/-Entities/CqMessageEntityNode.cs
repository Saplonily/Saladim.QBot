using System.Text.Json;
using System.Text.Json.Nodes;
using SaladimQBot.Core;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;


public abstract class CqMessageEntityNode : IMessageEntityNode
{
    [Ignore]
    public abstract MessageNodeType NodeType { get; }

    [Ignore]
    public CqCodeType CqCodeType { get => NodeType.Cast<CqCodeType>(); }

    /// <summary>
    /// 获取键值对参数字典
    /// </summary>
    /// <returns>键值对字典</returns>
    public virtual IDictionary<string, string> GetParamsDictionary()
    {
        JsonNode? node = JsonSerializer.SerializeToNode(this, GetType(), CqJsonOptions.Instance);
        if (node is null) throw new Exception("Error occurred when serialize a CqMessageEntityNode. Node is null.");
        StringDictionary? dic = JsonSerializer.Deserialize<StringDictionary>(node, CqJsonOptions.Instance);
        if (dic is null) throw new ArgumentException("Failed to serialize a node to params dictionary.");
        return dic;
    }
}



//不作为abstract是因为json反序列化器需要一个构造函数
//哈?
//2022-11-14 11:50:35 ↑ 我忘了我当时说这两句话什么意思了,反正现在没啥bug