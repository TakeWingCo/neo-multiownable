using Neo;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Neo.Lux.Core;
using Neo;
using NeoOriginal = Neo;


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

        public static string LoadContract()
        {
            var noparamAVM = System.IO.File.ReadAllBytes(Settings.Data.PathToContractFile);
            var str = Helper.ToHexString(noparamAVM);

            
            NeoOriginal.VM.ScriptBuilder sb = new NeoOriginal.VM.ScriptBuilder();
            sb.EmitPush(12);
            sb.EmitPush(14);
            sb.EmitPush(2);
            sb.Emit(NeoOriginal.VM.OpCode.PACK);
            sb.EmitPush("param1");
            var _params = sb.ToArray();
            var str2 = NeoOriginal.Helper.ToHexString(_params);

            Console.WriteLine("AVM=" + str2 + str);
            Console.ReadLine();
            
            return "";
        }
    }
}
