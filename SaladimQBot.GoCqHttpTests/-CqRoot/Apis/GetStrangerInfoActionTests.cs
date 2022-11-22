using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaladimQBot.GoCqHttp.Apis.Tests;

[TestClass()]
public class GetStrangerInfoActionTests
{
    [TestMethod()]
    public void EqualsTest()
    {
        GetStrangerInfoAction a = new()
        {
            UseCache = true,
            UserId = 114514
        };
        GetStrangerInfoAction b = new()
        {
            UseCache = true,
            UserId = 114514
        };
        Assert.AreEqual(a, b);
        a.UseCache = false;
        Assert.AreNotEqual(a, b);
        a.UseCache = true;
        a.UserId = 1919810;
        Assert.AreNotEqual(a, b);
        b.UserId = 1919810;
        Assert.AreEqual(a, b);
    }
}