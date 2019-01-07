using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Debugger.Core.Models;
using Neo.Debugger.Core.Utils;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Neo.VM.Types;

namespace TakeWing.Neo.Multiownable.Tests
{
    [TestClass]
    public class Multiownable
    {
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

        private static DebugManager Debugger;
        private static TestContext TestContextInstance;
        private static System.String PublicKey;

        private void OnLogMessage(System.String msg)
        {
            TestContext.WriteLine(msg);
        }

        [TestInitialize]
        public void TestSetup()
        {
            Debugger = new DebugManager();
            Debugger.LoadContract(@"..\..\..\TakeWing.Neo.Multiownable.SmartContractsForTests\bin\Debug\TakeWing.Neo.Multiownable.SmartContractsForTests.avm");

            PublicKey = "02b3622bf4017bdfe317c58aed5f4c753f206b7db896046fa7d774bbc4bf7f8dc2";
            var keyPair = DebuggerUtils.GetKeyFromString("347bc2bd9eb7b9f41a217a26dc5a3d2a3c25ece1c8bff1d5a146aaf4156e3436");
            Runtime.invokerKeys = keyPair;

            Runtime.OnLogMessage = OnLogMessage;
        }

        [TestMethod]
        public void TransferOwnershipTest()
        {
            var operation = "TransferOwnership";

            var args = System.String.Format("\"{0}\",[\"0x{1}\", 1, \"0x{1}\"]", operation, PublicKey);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void GetNumberOfOwnersTest()
        {
            TransferOwnershipTest();

            var operation = "GetNumberOfOwners";

            var args = System.String.Format("\"{0}\",[]", operation);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());
        }

        [TestMethod]
        public void GetIndexByOwnerTest()
        {
            TransferOwnershipTest();

            var operation = "GetIndexByOwner";

            var args = System.String.Format("\"{0}\",[\"0x{1}\"]", operation, PublicKey);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());
        }

        [TestMethod]
        public void GetOwnerByIndexTest()
        {
            TransferOwnershipTest();

            var operation = "GetOwnerByIndex";

            var args = System.String.Format("\"{0}\",[1]", operation);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            CollectionAssert.AreEqual(PublicKey.HexToBytes(), result.GetByteArray());
        }

        [TestMethod]
        public void GetGenerationOfOwnersTest()
        {
            TransferOwnershipTest();

            var operation = "GetGenerationOfOwners";

            var args = System.String.Format("\"{0}\",[]", operation);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());
        }

        [TestMethod]
        public void IsOwnerTest()
        {
            TransferOwnershipTest();

            var operation = "IsOwner";

            var args = System.String.Format("\"{0}\",[\"0x{1}\"]", operation, PublicKey);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void GetAllOwnersTest()
        {
            TransferOwnershipTest();

            var operation = "GetAllOwners";

            var args = System.String.Format("\"{0}\",[]", operation);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput() as Array;

            CollectionAssert.AreEqual(PublicKey.HexToBytes(), result[0].GetByteArray());
        }

        [TestMethod]
        public void UnknownOperationTest()
        {
            var operation = "Unknown";

            var args = System.String.Format("\"{0}\",[]", operation);
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsFalse(result.GetBoolean());
        }
    }
}