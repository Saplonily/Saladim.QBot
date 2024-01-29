using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaladimQBot.GoCqHttp.Tests;

[TestClass()]
public class EnumAttributeCacherTests
{
    [TestMethod()]
    public void GetStrAttrFromEnumTest()
    {
        TestEnum e = TestEnum.EnumA;
        string s = EnumAttributeCacher.GetStrAttrFromEnum(e);
        Assert.AreEqual("eA", s);
    }

    [TestMethod()]
    public void GetIntAttrFromEnumTest()
    {
        TestEnum e = TestEnum.EnumC;
        int s = EnumAttributeCacher.GetIntAttrFromEnum(e);
        Assert.AreEqual(114514, s);
    }

    public enum TestEnum
    {
        Invalid,
        [FormInPost("eA")]
        EnumA,
        [FormInPost("eB")]
        EnumB,
        [FormInPost(114514)]
        EnumC
    }

}