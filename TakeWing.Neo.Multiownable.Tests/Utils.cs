using LunarParser;
using LunarParser.JSON;

namespace TakeWing.Neo.Multiownable.Tests
{
    class Utils
    {
        public static DataNode GetArgsListAsNode(string argList)
        {
            var node = JSONReader.ReadFromString("{\"params\": [" + argList + "]}");
            return node.GetNode("params");
        }
    }
}
