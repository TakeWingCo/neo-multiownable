using System.Numerics;
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
        /// <summary>
        /// Information to unit test
        /// </summary>
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

        /// <summary>
        /// Debugger for tests
        /// </summary>
        private DebugManager Debugger;
        private TestContext TestContextInstance;
        
        private static KeyPair[] keyPairs;

        /// <summary>
        /// Write message to TestContext
        /// </summary>
        /// <param name="msg">Message</param>
        private void OnLogMessage(System.String msg)
        {
            TestContext.WriteLine(msg);
        }

        /// <summary>
        /// Sets 5 test KeyPairs
        /// </summary>
        /// <param name="testContext">Not used</param>
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

        /// <summary>
        /// Load test contract to Debugger
        /// Choice KeyPair[0] to use
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            Debugger = new DebugManager();
            Debugger.LoadContract(@"..\..\..\TakeWing.Neo.Multiownable.SmartContractsForTests\bin\Debug\TakeWing.Neo.Multiownable.SmartContractsForTests.avm");
            Runtime.invokerKeys = keyPairs[0];

            Runtime.OnLogMessage = OnLogMessage;
        }

        /// <summary>
        /// Successful execution of TransferOwnership for "zero generation"
        /// </summary>
        [TestMethod]
        public void TransferOwnership_ZeroGeneration_TransferSuccessful()
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

            // Check if setting up new owners is successful.
            Assert.IsTrue(result.GetBoolean(), "Transfer should have been successful");
        }

        /// <summary>
        /// Successful execution of TransferOwnership for "non-zero generation" and check if correct owner:
        ///     Run TransferOwnership_ZeroGeneration_TransferSuccessful()
        ///     Running TransferOwnership in turn for 0, 1 and 2 KeyPairs as an initiator and KeyPairs[2] as owner
        ///     Check if correct number of owners.
        ///     Check if correct owner.
        ///     Check if transferring is successful.
        /// </summary>
        [TestMethod]
        public void TransferOwnership_NonZeroGeneration_TransferSuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "TransferOwnership";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote.
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", 1, \"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Check if correct number of owners.
            operation = "GetNumberOfOwners";

            args = $"\"{operation}\",[]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger());

            // Check if correct owner.
            operation = "IsOwner";

            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            result = Debugger.Emulator.GetOutput();

            // Check if transferring is successful.
            Assert.IsTrue(result.GetBoolean(), "Transfer should have been successful");
        }

        /// <summary>
        /// Successful voting with enough votes (3 votes out of 5)
        /// </summary>
        [TestMethod]
        public void Voting_EnoughVotes_VotingSuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            //Third vote.
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished successfully with enough votes.
            Assert.IsTrue(result.GetBoolean(), "Voting should have been successful");
        }

        /// <summary>
        /// Unsuccessful voting with not enough votes (2 votes out of 5)
        /// </summary>
        [TestMethod]
        public void Voting_NotEnoughVotes_VotingUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is unfinished with not enough votes.
            Assert.IsFalse(result.GetBoolean(), "Voting should not have been successful");
        }

        /// <summary>
        /// Unsuccessful voting with not enough votes until Timeout is over
        /// (2 owners managed to vote and 1 did not have time to vote before Timeout is over)
        /// </summary>
        [TestMethod]
        public void Voting_TimeoutPassed_VotingUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time.
            Debugger.Emulator.timestamp += 2000;

            // Third vote.
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is unfinished after expire time.
            Assert.IsFalse(result.GetBoolean(), "Voting should not have been successful");
        }

        /// <summary>
        /// Successful voting at the end of the timeout and after that a sufficient number of votes
        /// </summary>
        [TestMethod]
        public void Voting_TimeoutPassedNewVotingStarted_VotingSuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time.
            Debugger.Emulator.timestamp += 2000;

            // Expired vote.
            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // First vote.
            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote.
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished successfully after starting it after timeout.
            Assert.IsTrue(result.GetBoolean(), "Voting should have been successful");
        }

        /// <summary>
        /// Unsuccessful voting with enough votes (4 votes out of 5), but all voting with different arguments
        /// </summary>
        [TestMethod]
        public void Voting_DifferentVotingParams_VotingUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 1000, 1, \"string\"]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Third vote.
            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 5, 500, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Forth vote.
            Runtime.invokerKeys = keyPairs[3];
            args = $"\"{operation}\",[\"0x{keyPairs[3].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 4, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if different arguments result in different voting.
            Assert.IsFalse(result.GetBoolean(), "Voting should not have been successful");
        }

        /// <summary>
        /// Unsuccessful voting with enough votes (3 votes out of 5),
        /// but one of them managed to pick up a vote back (actually 2 votes out of 5)
        /// </summary>
        [TestMethod]
        public void CancelVote_OneCancellation_VotingUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Cancel first vote.
            operation = "CancelCall";

            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            //Third vote.
            operation = "Call";

            Runtime.invokerKeys = keyPairs[2];
            args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished unsuccessfully after one cancellation.
            Assert.IsFalse(result.GetBoolean(), "Voting should have not been successful");
        }

        /// <summary>
        /// After Timeout is over, the vote cannot be canceled.
        /// </summary>
        [TestMethod]
        public void CancelVote_CancellationAfterTimeout_CancellationUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Increase time.
            Debugger.Emulator.timestamp += 2000;

            // Cancel first vote.
            operation = "CancelCall";

            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if cancellation is unsuccessful after timeout.
            Assert.IsFalse(result.GetBoolean(), "Cancellation should have failed");
        }

        /// <summary>
        /// Vote cannot be canceled if you submit different arguments
        /// </summary>
        [TestMethod]
        public void CancelVote_CancellationInAnotherVoting_CancellationUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Cancel first vote.
            operation = "CancelCall";

            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if cancellation is unsuccessful in another voting.
            Assert.IsFalse(result.GetBoolean(), "Cancellation should have failed");
        }

        /// <summary>
        /// Cancellation may reduce the number of votes to zero
        /// </summary>
        [TestMethod]
        public void CancelVote_CancellationToZero_VotingUnsuccessful()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "Call";

            // First vote.
            var args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Cancel first vote.
            operation = "CancelCall";

            Runtime.invokerKeys = keyPairs[0];
            args = $"\"{operation}\",[\"0x{keyPairs[0].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            // Second vote.
            operation = "Call";

            Runtime.invokerKeys = keyPairs[1];
            args = $"\"{operation}\",[\"0x{keyPairs[1].CompressedPublicKey.ToHexString()}\", \"Boolean TransferOwnership(Byte[], Byte[][])\", 3, 1000, 0]";
            inputs = DebuggerUtils.GetArgsListAsNode(args);

            script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if voting is finished unsuccessfully after all votes canceled.
            Assert.IsFalse(result.GetBoolean(), "Voting should have been unsuccessful");
        }

        /// <summary>
        /// For GetIndexByOwner request with owner exists, returns index is correct
        /// </summary>
        [TestMethod]
        public void GetIndexByOwner_OwnerExists_IndexCorrect()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetIndexByOwner";

            var args = $"\"{operation}\",[\"0x{keyPairs[2].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if returned index is correct with existent owner.
            Assert.AreEqual(BigInteger.Parse("3"), result.GetBigInteger(), "Index should have been 3");
        }

        /// <summary>
        /// For GetIndexByOwner request with owner non-exists, returns index = ""
        /// </summary>
        [TestMethod]
        public void GetIndexByOwner_OwnerNotExists_IndexNull()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetIndexByOwner";

            var args = $"\"{operation}\",[\"0xabcd\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if returned index is null with non existent owner.
            Assert.AreEqual("", result.GetString(), "Index should have been null");
        }

        /// <summary>
        /// For GetOwnerByIndex request with the correct index, returns owner is correct
        /// </summary>
        [TestMethod]
        public void GetOwnerByIndex_IndexExists_OwnerCorrect()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetOwnerByIndex";

            var args = $"\"{operation}\",[2]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if return owner is correct with existent index.
            CollectionAssert.AreEqual(keyPairs[1].CompressedPublicKey, result.GetByteArray(), "Owner should have been correct");
        }

        /// <summary>
        /// For GetOwnerByIndex request with the incorrect index, returns owner = ""
        /// </summary>
        [TestMethod]
        public void GetOwnerByIndex_IndexNotExists_OwnerNull()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetOwnerByIndex";

            var args = $"\"{operation}\",[10]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if returned owner is null with non existent index.
            Assert.AreEqual("", result.GetString(), "Owner should have been null");
        }

        /// <summary>
        /// For IsOwner request with owner exists, returns true
        /// </summary>
        [TestMethod]
        public void IsOwnerTest_PublicKeyOwner_Owner()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "IsOwner";

            var args = $"\"{operation}\",[\"0x{keyPairs[3].CompressedPublicKey.ToHexString()}\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if existent owner is considered as owner.
            Assert.IsTrue(result.GetBoolean(), "Public key should have been considered owner's");
        }

        /// <summary>
        /// For IsOwner request with owner non-exists, returns false
        /// </summary>
        [TestMethod]
        public void IsOwnerTest_PublicKeyNotOwner_NotOwner()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "IsOwner";

            var args = $"\"{operation}\",[\"0xabcd\"]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if non existent owner is not considered as owner.
            Assert.IsFalse(result.GetBoolean(), "Public key should not have been considered owner's");
        }

        /// <summary>
        /// GetAllOwners request correctly returns all owners
        /// </summary>
        [TestMethod]
        public void GetAllOwners_KnownOwners_OwnersCorrect()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetAllOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput() as Array;

            // Check if returned owners are correct.
            for(var i = 0; i < 5; i++)
                CollectionAssert.AreEqual(keyPairs[i].CompressedPublicKey, result[i].GetByteArray(), "Owner should have been correct");
        }

        /// <summary>
        /// GetGenerationOfOwners request correctly returns generation (= 1) for zero-generation.
        /// </summary>
        [TestMethod]
        public void GetGenerationOfOwners_FirstGeneration_GenerationCorrect()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetGenerationOfOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if generation of owners is correct.
            Assert.AreEqual(BigInteger.Parse("1"), result.GetBigInteger(), "Generation of owners should have been 1");
        }

        /// <summary>
        /// GetNumberOfOwners request correctly returns the number of owners (=5)
        /// </summary>
        [TestMethod]
        public void GetNumberOfOwners_KnownNumberOfOwners_NumberOfOwnersCorrect()
        {
            TransferOwnership_ZeroGeneration_TransferSuccessful();

            var operation = "GetNumberOfOwners";

            var args = $"\"{operation}\",[]";
            var inputs = DebuggerUtils.GetArgsListAsNode(args);

            var script = Debugger.Emulator.GenerateLoaderScriptFromInputs(inputs, Debugger.ABI);
            Debugger.Emulator.Reset(script, Debugger.ABI, "Main");
            Debugger.Emulator.Run();

            var result = Debugger.Emulator.GetOutput();

            // Check if number of owners is correct.
            Assert.AreEqual(BigInteger.Parse("5"), result.GetBigInteger(), "Number of owners should have been 3");
        }
    }
}