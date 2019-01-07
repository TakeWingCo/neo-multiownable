using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Debugger.Core.Utils;
using Neo.Emulation.API;
using Neo.Lux.Utils;
using Neo.VM.Types;
using Neo.Emulation;

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

        private DebugManager Debugger;
        private TestContext TestContextInstance;

        private static System.String[] PublicKeys;

        private void OnLogMessage(System.String msg)
        {
            TestContext.WriteLine(msg);
        }

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            PublicKeys = new System.String[5];
            PublicKeys[0] = "0218c0828e52456243ef56e3df865a55f3c6f83e291ff689a5a841a1bac6ab646b";
            PublicKeys[1] = "020e55e594e4aa0c7d2759327b45cb640a595978507f7a67ab1e401657efb452ce";
            PublicKeys[2] = "027f13d9e6ce080d18587e505f70e2f0667c3a59fae80bedb9b4f29575821ff7b0";
            PublicKeys[3] = "0383592150122b607448793522aa596f366623036540d8d1354914cf4057e19ad0";
            PublicKeys[4] = "032fd6fa29d1504731f2292a37527f18a5046b2c627a954f2aaa2b359694613a59";

            //var keyPair = DebuggerUtils.GetKeyFromString("347bc2bd9eb7b9f41a217a26dc5a3d2a3c25ece1c8bff1d5a146aaf4156e3436");
            //Runtime.invokerKeys = keyPair;
        }

        [TestInitialize]
        public void TestSetup()
        {
            Debugger = new DebugManager();
            Debugger.LoadContract(@"..\..\..\TakeWing.Neo.Multiownable.SmartContractsForTests\bin\Debug\TakeWing.Neo.Multiownable.SmartContractsForTests.avm");
            Debugger.Emulator.checkWitnessMode = CheckWitnessMode.AlwaysTrue;

            Runtime.OnLogMessage = OnLogMessage;
        }

        [TestMethod]
        public void SetOwnershipTest()
        {
            var operation = "TransferOwnership";

            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", 5, \"0x{PublicKeys[0]}\", \"0x{PublicKeys[1]}\", \"0x{PublicKeys[2]}\", \"0x{PublicKeys[3]}\", \"0x{PublicKeys[4]}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void TransferOwnershipTest()
        {
            SetOwnershipTest();

            var operation = "TransferOwnership";

            // First vote
            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", 1, \"0x{PublicKeys[2]}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            args = $"\"{operation}\",[\"0x{PublicKeys[1]}\", 1, \"0x{PublicKeys[2]}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote
            args = $"\"{operation}\",[\"0x{PublicKeys[2]}\", 1, \"0x{PublicKeys[2]}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Check if correct number of owners
            operation = "GetNumberOfOwners";

            args = $"\"{operation}\",[]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());

            // Check if correct owner
            operation = "IsOwner";

            args = $"\"{operation}\",[\"0x{PublicKeys[2]}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void EnoughVotesCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            args = $"\"{operation}\",[\"0x{PublicKeys[1]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            //Third vote
            args = $"\"{operation}\",[\"0x{PublicKeys[2]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void NotEnoughVotesCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            args = $"\"{operation}\",[\"0x{PublicKeys[1]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void TimeoutCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            args = $"\"{operation}\",[\"0x{PublicKeys[1]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time
            Debugger.Emulator.timestamp += 2000;

            // Third vote
            args = $"\"{operation}\",[\"0x{PublicKeys[2]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void DifferentVotingsTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{PublicKeys[0]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            args = $"\"{operation}\",[\"0x{PublicKeys[1]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 1, \"string\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote
            args = $"\"{operation}\",[\"0x{PublicKeys[2]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 500, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Forth vote
            args = $"\"{operation}\",[\"0x{PublicKeys[3]}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 4, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void GetCorrectIndexByOwnerTest()
        {
            SetOwnershipTest();

            var operation = "GetIndexByOwner";

            var args = $"\"{operation}\",[\"0x{PublicKeys[2]}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("3"), result.GetBigInteger());
        }

        [TestMethod]
        public void GetWrongIndexByOwnerTest()
        {
            SetOwnershipTest();

            var operation = "GetIndexByOwner";

            var args = $"\"{operation}\",[\"0xabcd\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual("", result.GetString());
        }

        [TestMethod]
        public void GetCorrectOwnerByIndexTest()
        {
            SetOwnershipTest();

            var operation = "GetOwnerByIndex";

            var args = $"\"{operation}\",[2]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            CollectionAssert.AreEqual(PublicKeys[1].HexToBytes(), result.GetByteArray());
        }

        [TestMethod]
        public void GetWrongOwnerByIndexTest()
        {
            SetOwnershipTest();

            var operation = "GetOwnerByIndex";

            var args = $"\"{operation}\",[10]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual("", result.GetString());
        }

        [TestMethod]
        public void IsCorrectOwnerTest()
        {
            SetOwnershipTest();

            var operation = "IsOwner";

            var args = $"\"{operation}\",[\"0x{PublicKeys[3]}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void IsWrongOwnerTest()
        {
            SetOwnershipTest();

            var operation = "IsOwner";

            var args = $"\"{operation}\",[\"0xabcd\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void GetAllOwnersTest()
        {
            SetOwnershipTest();

            var operation = "GetAllOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput() as Array;

            for(var i = 0; i < 5; i++)
                CollectionAssert.AreEqual(PublicKeys[i].HexToBytes(), result[i].GetByteArray());
        }

        [TestMethod]
        public void GetGenerationOfOwnersTest()
        {
            SetOwnershipTest();

            var operation = "GetGenerationOfOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());
        }

        [TestMethod]
        public void GetNumberOfOwnersTest()
        {
            SetOwnershipTest();

            var operation = "GetNumberOfOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("5"), result.GetBigInteger());
        }
    }
}