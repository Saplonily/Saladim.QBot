using System.Text;
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
    /// 格式化为Cq格式的字符串
    /// </summary>
    /// <returns></returns>
    public abstract string CqStringify();
}



//不作为abstract是因为json反序列化器需要一个构造函数
//哈?
//2022-11-14 11:50:35 ↑ 我忘了我当时说这两句话什么意思了,反正现在没啥bug