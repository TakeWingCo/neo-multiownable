using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TakeWing.Neo.Multiownable.Tests
{
    public class Response
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }

           
    }

    public class Result
    {
        [JsonProperty("script")]
        public string Script { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("gas_consumed")]
        public string GasConsumed { get; set; }

        [JsonProperty("stack")]
        public Stack[] Stack { get; set; }

        [JsonProperty("tx")]
        public string Tx { get; set; }
    }

    public class Stack
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
