using Neo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static int RequestId = 1;
        public static string responseContent;
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
                Logger.InitLogger();
                var  client = new HttpClient();
                Request RpcRequest = new Request()
                {
                    JsonRpc = "2.0",
                    Id = RequestId.ToString(),
                    Method = "invokescript",
                    Params = new []{LoadContractScript(),""}
                };
                
                //string RpcJson = JsonConvert.SerializeObject(RpcRequestDictionary, Formatting.Indented);
                string json = await Task.Run(() => JsonConvert.SerializeObject(RpcRequest));
                Logger.Log.Info(json);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                using (var httpClient = new HttpClient())
                {
                    // Error here
                    var httpResponse = await httpClient.PostAsync("http://" + Data.IpAddresNode + "/", httpContent);
                    if (httpResponse.Content != null)
                    {
                        // Error Here
                        responseContent = await httpResponse.Content.ReadAsStringAsync();
                        Logger.Log.Info(responseContent);
                        var response = JsonResponseToStringConverter(responseContent);  
                    }
                    return responseContent;
                }
        }

        public static Response JsonResponseToStringConverter(string ResponseJson)
        {
            Response response = new Response();
            response = JsonConvert.DeserializeObject<Response>(ResponseJson);
            Logger.Log.Info(response.Result.State);
            return response;
        }
    }

}
