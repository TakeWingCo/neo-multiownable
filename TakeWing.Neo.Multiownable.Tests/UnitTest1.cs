using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Emulation;
using Neo.Emulation.API;

namespace TakeWing.Neo.Multiownable.Tests
{
    [TestClass]
    public class MultiownableTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Settings.Init();
        }

        [TestMethod]
        public void Settings_init()
        {
            Assert.AreNotEqual("0.0.0.0",Settings.data.IP_addres_node);
            Assert.AreNotEqual("contract_name.avm",Settings.data.Path_to_contract_file);
        }
    }
}

