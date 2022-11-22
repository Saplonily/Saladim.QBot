using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaladimQBot.GoCqHttp.Tests
{
    [TestClass]
    public class CqMessageUnimplementedNodeJsonConverterTests
    {
        [TestMethod]
        public void WriteTest()
        {
            string name = "somethingName";
            Dictionary<string, string> strDic = new()
            {
                ["param1"] = "114514",
                ["param2"] = "1919810"
            };
            CqMessageUnimplementedNode node = new(name, strDic);
            string str = JsonSerializer.Serialize(node);
            Assert.AreEqual("""{"type":"somethingName","data":{"param1":"114514","param2":"1919810"}}""", str);
        }

        [TestMethod]
        public void ReadTest()
        {
            string str = """{"type":"somethingName","data":{"param1":"114514","param2":"1919810"}}""";
            var node = JsonSerializer.Deserialize<CqMessageUnimplementedNode>(str);

            Assert.AreEqual("somethingName", node?.Name);
            Assert.AreEqual("114514", node?.Params["param1"]);
        }
    }
}