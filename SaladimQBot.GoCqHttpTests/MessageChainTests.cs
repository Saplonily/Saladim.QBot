using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaladimQBot.GoCqHttp;

namespace SaladimQBot.GoCqHttp.Tests;

[TestClass()]
public class MessageChainTests
{
    [TestMethod()]
    public void FromModelTest()
    {
        CqMessageChainModel chainModel = new()
        {
            ChainNodeModels =
            {
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} )
            }
        };
        var cm = MessageChain.FromModel(null!, chainModel);
        Assert.AreEqual(expected: 3, actual: cm.MessageChainNodes.Count);
    }

    [TestMethod()]
    public void FromModelTest2()
    {
        CqMessageChainModel chainModel = new()
        {
            ChainNodeModels =
            {
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
                new CqMessageChainNodeModel(CqCodeType.Text,new Dictionary<string,string>(){ ["text"] = "114514"} ),
                new CqMessageChainNodeModel(CqCodeType.Unimplemented,new Dictionary<string,string>()),
            }
        };
        var cm = MessageChain.FromModel(null!, chainModel);
        Assert.AreEqual(expected: 9, actual: cm.MessageChainNodes.Count);
    }
}