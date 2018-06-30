using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakeWing.Neo.Multiownable.Tests
{
    public static class Settings
    {
        public static Data data;
        public static void Init()
        {
            if (File.Exists(@"..\..\config.json"))
            {
                string path = Directory.GetCurrentDirectory();
                data = JsonConvert.DeserializeObject<Data>
                    (File.ReadAllText(@"..\..\config.json"));
            }
            else
            {
                throw new Exception("Invalid config file, expected path: " + @"..\..\config.json");
            }
            if (File.Exists(@"..\..\Contracts\" + data.Path_to_contract_file))
            {
            }
            else
            {
                throw new Exception("Invalid config file, expected path: " + @"..\..\Contracts\" + data.Path_to_contract_file);
            }

        }
    }
}
