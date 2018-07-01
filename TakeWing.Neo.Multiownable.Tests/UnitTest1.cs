using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Emulation;
using Neo.Emulation.API;


namespace TakeWing.Neo.Multiownable.Tests
{
    [TestClass]
    public class MultiownableTests
    {
        private TestContext TestContextInstance;

        public TestContext TestContext
        {
            get
            {
                return TestContextInstance;
            }
            set
            {
                TestContextInstance = value;
            }
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Settings.Init();
        }

        [TestMethod]
        public void SettingsInitTest()
        {
            Assert.AreNotEqual("0.0.0.0", Settings.Data.IpAddresNode);
            Assert.AreNotEqual("contract_name.avm", Settings.Data.PathToContractFile);
        }

        [TestMethod]
        public void SettingsLoadContractScriptTest()
        {
            Object[] ParamsArray= {"TestValue1","TestValue2"};
            var ContractScript = Settings.LoadContractScript("TestCase1",ParamsArray);
            TestContextInstance.WriteLine(ContractScript);
            Assert.IsNotNull(ContractScript);
            
        }

        [TestMethod]
        public async Task SettingsConnectToTheRemoteNodeByRpcTestAsync()
        {
            string ResponseRpc = await Settings.ConnectToTheRemoteNodeByRpcAsync();
            //throw new Exception("Result is: " + ResponseRpc);
            Assert.IsNotNull(ResponseRpc);
            TestContextInstance.WriteLine(ResponseRpc);
        }
    }
}

