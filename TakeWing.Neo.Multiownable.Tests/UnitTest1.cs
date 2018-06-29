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
        private static Emulator emulator;
        private static ABI abiFile;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            string path = Directory.GetCurrentDirectory();
            var avmBytes = File.ReadAllBytes(string.Format(@"{0}\Contracts\PoCNeoContract.avm", path));
            var chain = new Blockchain();
            emulator = new Emulator(chain);
            var address = chain.DeployContract("test", avmBytes);
            emulator.SetExecutingAccount(address);
            emulator.checkWitnessMode = CheckWitnessMode.AlwaysTrue;

            string pathToAbi = string.Format(@"{0}\Contracts\PoCNeoContract.abi.json", path);
            abiFile = new ABI(pathToAbi);

        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}

