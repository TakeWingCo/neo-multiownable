﻿using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Debugger.Core.Utils;
using Neo.Emulation.API;
using Neo.Lux.Cryptography;
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

        private DebugManager Debugger;
        private TestContext TestContextInstance;
        
        private static KeyPair[] keyPairs;

        private void OnLogMessage(System.String msg)
        {
            TestContext.WriteLine(msg);
        }

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            keyPairs = new KeyPair[5];
            keyPairs[0] = DebuggerUtils.GetKeyFromString("55bf86005a406de10be23a63cfc04770c4902b830d3b83071cdf13a240a645b2");
            keyPairs[1] = DebuggerUtils.GetKeyFromString("3cec35ae9900c69ae369cd793486c63814ad3380cc5de62d3daec45c4da1c4ee");
            keyPairs[2] = DebuggerUtils.GetKeyFromString("3a7764d6779f328bcad737fa08e2fd331aba45e5a3b9104923be233e7d0a569f");
            keyPairs[3] = DebuggerUtils.GetKeyFromString("dcecf06f5cb3b6f39678b4f550e5a65789b7f6e8931a339a525e9c2e64e5394c");
            keyPairs[4] = DebuggerUtils.GetKeyFromString("132a7996a6183221d21a767b77fe3afc331e5fe553d28a633364ae951a1fa841");
        }

        [TestInitialize]
        public void TestSetup()
        {
            Debugger = new DebugManager();
            Debugger.LoadContract(@"..\..\..\TakeWing.Neo.Multiownable.SmartContractsForTests\bin\Debug\TakeWing.Neo.Multiownable.SmartContractsForTests.avm");
            Runtime.invokerKeys = keyPairs[0];

            Runtime.OnLogMessage = OnLogMessage;
        }

        [TestMethod]
        public void SetOwnershipTest()
        {
            var operation = "TransferOwnership";

            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", 5, \"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", " +
                                                                                                  $"\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", " +
                                                                                                  $"\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", " +
                                                                                                  $"\"0x{keyPairs[3].CompressedPublicKey.ToHexString()}\", " +
                                                                                                  $"\"0x{keyPairs[4].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if setting up new owners is successful
            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void TransferOwnershipTest()
        {
            SetOwnershipTest();

            var operation = "TransferOwnership";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
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

            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            result = Debugger.Emulator.GetOutput();

            // Check if transferring is successful
            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void EnoughVotesCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            //Third vote
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished successfully with enough votes
            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void NotEnoughVotesCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is unfinished with not enough votes
            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void TimeoutCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time
            Debugger.Emulator.timestamp += 2000;

            // Third vote
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is unfinished after expire time
            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void NewVotingCallTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time
            Debugger.Emulator.timestamp += 2000;

            // First vote
            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished successfully after starting it after timeout
            Assert.IsTrue(result.GetBoolean());
        }

        [TestMethod]
        public void DifferentVotingsTest()
        {
            SetOwnershipTest();

            var operation = "Call";

            // First vote
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 1, \"string\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 500, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Forth vote
            Runtime.invokerKeys = keyPairs[3];
            args = $"\"{operation}\",[\"0x{keyPairs[3].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 4, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if different arguments result in different voting
            Assert.IsFalse(result.GetBoolean());
        }

        [TestMethod]
        public void GetCorrectIndexByOwnerTest()
        {
            SetOwnershipTest();

            var operation = "GetIndexByOwner";

            var args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if returned index is correct with existent owner
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

            // Check if returned index is null with non existent owner
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

            // Check if return owner is correct with existent index
            CollectionAssert.AreEqual(keyPairs[1].CompressedPublicKey, result.GetByteArray());
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

            // Check if returned owner is null with non existent index
            Assert.AreEqual("", result.GetString());
        }

        [TestMethod]
        public void IsCorrectOwnerTest()
        {
            SetOwnershipTest();

            var operation = "IsOwner";

            var args = $"\"{operation}\",[\"0x{keyPairs[3].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if existent owner is considered as owner
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

            // Check if non existent owner is not considered as owner
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

            // Check if returned owners are correct
            for(var i = 0; i < 5; i++)
                CollectionAssert.AreEqual(keyPairs[i].CompressedPublicKey, result[i].GetByteArray());
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

            // Check if generation of owners is correct
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

            // Check if number of owners is correct
            Assert.AreEqual(BigInteger.Parse("5"), result.GetBigInteger());
        }
    }
}