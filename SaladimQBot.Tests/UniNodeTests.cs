using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaladimQBot.Core;

namespace SaladimQBot.Tests;

[TestClass]
public class UniNodeTests
{
    // 单主参测试
    [TestMethod]
    public void ParseTest1()
    {
        var node = GenericUniNode.Parse(null!, "<at:2748166392>");
        Assert.AreEqual("at", node.Name);
        var dic = node.Deconstruct();
        Assert.AreEqual("2748166392", node.PrimaryValue);
        Assert.AreEqual(0, dic.Count);
    }

    // 自结束测试
    [TestMethod]
    public void ParseTest2()
    {
        var node = GenericUniNode.Parse(null!, "<fuckyou>");
        Assert.AreEqual("fuckyou", node.Name);
        Assert.AreEqual(0, node.Deconstruct().Count);
    }

    [TestMethod]
    public void ParseTest3()
    {
        var node = GenericUniNode.Parse(null!, "<at,id=2748166392>");
        Assert.AreEqual("at", node.Name);
        var dic = node.Deconstruct();
        Assert.AreEqual("2748166392", dic["id"]);
        Assert.AreEqual(1, dic.Count);
    }

    [TestMethod]
    public void ParseTest4()
    {
        var node = GenericUniNode.Parse(null!, @"<text,content=\l\n\c\r\\>");
        Assert.AreEqual("text", node.Name);
        var dic = node.Deconstruct();
        Assert.AreEqual("<\n,>\\", dic["content"]);
        Assert.AreEqual(1, dic.Count);
    }

    [TestMethod]
    public void ParseTest5()
    {
        var node = GenericUniNode.Parse(null!, "<img:http://114514.com,type=1,size=75>");
        var dic = node.Deconstruct();
        Assert.AreEqual("img", node.Name);
        Assert.AreEqual("http://114514.com", node.PrimaryValue);
        Assert.AreEqual("1", dic["type"]);
        Assert.AreEqual("75", dic["size"]);
        Assert.AreEqual(2, dic.Count);
    }

    [TestMethod]
    public void EscapeTest()
    {
        string r = UniNode.Escape(" \n,,423,,,<423<43221312>432>");
        Assert.AreEqual(@"\s\n\c\c423\c\c\c\l423\l43221312\r432\r", r);
        Assert.AreEqual(UniNode.Deescape(@"\s\n\c\c423\c\c\c\l423\l43221312\r432\r"), " \n,,423,,,<423<43221312>432>");
    }

    [TestMethod]
    public void EscapeTest2()
    {
        string str = @"<6>\\\";
        Assert.AreEqual(@"\l6\r\\\\\\", UniNode.Escape(str));
        Assert.AreEqual(@"\l6\r\\\\", UniNode.Escape(UniNode.Deescape(str)));
        Assert.AreEqual(str, UniNode.Deescape(UniNode.Escape(str)));
        Assert.AreEqual(@"\l6\r\\\\\\", UniNode.Escape(UniNode.Deescape(UniNode.Escape(str))));
    }

    [TestMethod]
    public void ToFormatTest()
    {
        var node = GenericUniNode.Parse(null!, "<img:http://114514.com,type=1,size=75>");
        Assert.AreEqual("<img:http://114514.com,type=1,size=75>", node.ToFormattedText());
    }

    [TestMethod]
    public void ToFormatTest2()
    {
        var node = new GenericUniNode(null!, "test", new Dictionary<string, string>() { ["awa"] = "uwu", ["233"] = @"6\<,>" });
        Assert.AreEqual(@"<test,awa=uwu,233=6\\\l\c\r>", node.ToFormattedText());
    }

    [TestMethod]
    public void ToFormatTest3()
    {
        var node = new GenericUniNode(null!, "test", new Dictionary<string, string>() { ["awa"] = "uwu", ["233"] = @"6\<,>" },@"<\r>");
        Assert.AreEqual(@"<test:\l\\r\r,awa=uwu,233=6\\\l\c\r>", node.ToFormattedText());
    }
}
