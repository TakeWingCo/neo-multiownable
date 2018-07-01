using Neo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NeoOriginal = Neo;
using System.Net.Http;
using System.Threading.Tasks;


namespace TakeWing.Neo.Multiownable.Tests
{
    public static class Settings
    {
        public static Data Data;
        public static void Init()
        {
            if (File.Exists(@"..\..\config.json"))
            {
                var path = Directory.GetCurrentDirectory();
                Data = JsonConvert.DeserializeObject<Data>
                    (File.ReadAllText(@"..\..\config.json"));
            }
            else
            {
                throw new Exception("Invalid config file, expected path: " + @"..\..\config.json");
            }
            if (File.Exists(@"..\..\Contracts\" + Data.PathToContractFile))
            {
            }
            else
            {
                throw new Exception("Invalid config file, expected path: " + @"..\..\Contracts\" + Data.PathToContractFile);
            }
        }

        public static string LoadContractScript(String CallMethod = null, Object[] ParmsArray = null)
        {
            var NoparamAvm = File.ReadAllBytes(@"..\..\Contracts\" + Data.PathToContractFile);
            var AvmString = Helper.ToHexString(NoparamAvm);
            if (CallMethod == null && ParmsArray == null)
                return AvmString;
            NeoOriginal.VM.ScriptBuilder sb = new NeoOriginal.VM.ScriptBuilder();
            sb.EmitPush(CallMethod);
            foreach (var param in ParmsArray)
            {
                sb.EmitPush(Encoding.UTF8.GetBytes(param.ToString()));
            }
            sb.Emit(NeoOriginal.VM.OpCode.PACK);
            var _params = sb.ToArray();
            var ParamsString = NeoOriginal.Helper.ToHexString(_params);
            return ParamsString + AvmString;
        }

        public static async Task<string> ConnectToTheRemoteNodeByRpcAsync()
        {
            try
            {
                var  client = new HttpClient();
                Dictionary<String, String> RpcRequestDictionary = new Dictionary<string, String>
                {
                    {"jsonrpc", "2.0"},
                    {"method", "invokescript"},
                    {"params", "["+LoadContractScript()+"]"}
                };

                //string RpcJson = JsonConvert.SerializeObject(RpcRequestDictionary, Formatting.Indented);
                
                
                var body = new FormUrlEncodedContent(RpcRequestDictionary);

                var response = await client.PostAsync("http://" + Data.IpAddresNode + "/", body);

                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
            catch
            {
                return null;
            }
        }
    }
}
