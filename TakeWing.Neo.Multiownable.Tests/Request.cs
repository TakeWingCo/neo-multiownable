
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TakeWing.Neo.Multiownable.Tests
{
    class Request
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public string[] Params { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}