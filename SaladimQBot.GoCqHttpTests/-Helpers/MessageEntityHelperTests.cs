using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaladimQBot.GoCqHttp.Tests;

[TestClass()]
public class MessageEntityHelperTests
{
    [TestMethod()]
    public void CqEntity2RawStringTest()
    {
        //不带接收图片
        {
            CqMessageChainModel entity = new()
            {
                new CqMessageTextNode("你好这里是文本啊~[&nbsp属于是,]"),
                new CqMessageAtNode(2748166392),
                new CqMessageFaceNode(1)
            };
            string raw = MessageChainModelHelper.ChainToRawString(entity);
            string except = "你好这里是文本啊~[&nbsp属于是,]".CqEncode() +
                "[CQ:at,qq=2748166392][CQ:face,id=1]";
            Assert.AreEqual(except, raw);
        }

        //带接收图片
        {
            CqMessageChainModel entity = new()
            {
                new CqMessageTextNode("你好这里是文本啊~[&nbsp属于是,]"),
                new CqMessageAtNode(2748166392),
                new CqMessageFaceNode(1),
                new CqMessageImageReceiveNode(
                    "http://114514,233,[]pwp",
                    "I'm sb,[]",
                    ImageSendType.Normal,
                    ImageSendSubType.HotMeme,
                    ImageShowType.Invalid
                )
            };
            string raw = MessageChainModelHelper.ChainToRawString(entity);
            string except = "你好这里是文本啊~[&nbsp属于是,]".CqEncode() +
                "[CQ:at,qq=2748166392][CQ:face,id=1]";
            except += $"[CQ:image,url={"http://114514,233,[]pwp".CqEncode()}," +
                $"file={"I'm sb,[]".CqEncode()}" +
                "]";
            Assert.AreEqual(except, raw);
            entity.Add(new CqMessageImageReceiveNode(
                "http://nothing.com",
                "fileNameHere",
                ImageSendType.Normal,
                ImageSendSubType.Invalid,
                ImageShowType.Invalid
                ));
            except += $"[CQ:image,url={"http://nothing.com".CqEncode()},file=fileNameHere]";
        }

        //带未实现CQ码
        {
            CqMessageChainModel entity = new()
            {
                new CqMessageUnimplementedNode("at",new Dictionary<string,string>()
                {
                    ["qq"] = "2748166392"
                }),
                new CqMessageTextNode("114514[[CQ:at,&qq=2748166392]]")
            };
            string s = MessageChainModelHelper.ChainToRawString(entity);
            Assert.AreEqual("[CQ:at,qq=2748166392]" + "114514[[CQ:at,&qq=2748166392]]".CqEncode(), s);
        }
    }
}

internal static class Extensions
{
    public static string CqEncode(this string str)
    {
        return MessageChainModelHelper.CqEncode(str);
    }
}