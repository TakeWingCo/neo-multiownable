using System;
using System.Linq;
using System.Numerics;
using Neo.VM;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;


namespace TakeWing.Neo.Multiownable
{
	/// <summary>
	/// Library for easy multisig and consensus.
	/// </summary>
    public static class Multiownable
    {
		/// <summary>
		/// Sha256 by OpCode.
		/// </summary>
		/// <param name="byteArray"></param>
		/// <returns></returns>
		[OpCode(OpCode.SHA256)]
		public extern static Byte[] Sha256(Byte[] byteArray);

		/// <summary>
		/// Get number of owners.
		/// </summary>
		/// <returns>Number of owners</returns>
		public static Byte GetNumberOfOwners()
		{
			return Storage.Get(Storage.CurrentContext, "NumberOfOwners")[0];
		}

		/// <summary>
		/// Get owner by his index.
		/// </summary>
		/// <param name="index">Index of owner</param>
		/// <returns>Public key of owner</returns>
		public static Byte[] GetOwnerByIndex(Byte index)
		{
			var key = "Owners".AsByteArray();
			key.Append(index);

			return Storage.Get(Storage.CurrentContext, key);
		}

		/// <summary>
		/// Get index of owner.
		/// </summary>
		/// <param name="owner">Owner's public key</param>
		/// <returns>Owner's index</returns>
		public static Byte GetIndexByOwner(Byte[] owner)
		{
			var key = "IndexesOfOwners".AsByteArray();
			key = key.Concat(owner);

			return Storage.Get(Storage.CurrentContext, key)[0];
		}

		/// <summary>
		/// Get current generation of owners number.
		/// </summary>
		/// <returns>Generation of owners number</returns>
		public static UInt64 GetGenerationOfOwners()
		{
			return (UInt64)Storage.Get(Storage.CurrentContext, "GenerationOfOwners").AsBigInteger();
		}

		/// <summary>
		/// Get array of owners.
		/// </summary>
		/// <returns>Array of owners</returns>
		public static Byte[][] GetAllOwners()
		{
			byte ownersCount = GetNumberOfOwners();

			byte[][] result = new byte[][] { };
			for (byte i = 0; i < ownersCount; i++)
				result[i] = GetOwnerByIndex(i);

			return result;
		}

		/// <summary>
		/// Check if public key in owners list.
		/// </summary>
		/// <param name="publicKey">Public key</param>
		/// <returns>True if public key in owners list, false else</returns>
		public static Boolean IsOwner(Byte[] publicKey)
		{
			byte ownersCount = GetNumberOfOwners();
			Byte[][] ownersList = GetAllOwners();

			for (byte i = 0; i < ownersCount; i++)
				if (ownersList[i] == publicKey)
					return true;

			return false;
		}

		/// <summary>
		/// Transfer ownership to new owners list.
		/// </summary>
		/// <param name="initiator">Initiator's public key</param>
		/// <param name="newOwners">Array of public keys</param>
		/// <returns>Return true after successful transfer, else false</returns>
		public static Boolean TransferOwnership(Byte[] initiator, Byte[][] newOwners)
		{
			if (!IsOwner(initiator))
				return false;

			byte ownersCount = GetNumberOfOwners();
			if (ownersCount > 0)
			{
				// Clear current list of owners.
				for (byte i = 0; i < ownersCount; i++)
				{
					var key = "Owners".AsByteArray();
					key.Append(i);

					Storage.Delete(Storage.CurrentContext, key);
				}
			}

			// Set new list of owners.
			for (byte i = 0; i < newOwners.Length; i++)
			{
				var key = "Owners".AsByteArray();
				key.Append(i);

				Storage.Put(Storage.CurrentContext, key, newOwners[i]);
			}

			// Change generation.
			UInt64 newGeneration = GetGenerationOfOwners() + 1;
			Storage.Put(Storage.CurrentContext, "GenerationOfOwners", newGeneration);

			return true;
		}

		/// <summary>
		/// Check, that timeout doesn't expire and minimal required number of owners accepts call of function.
		/// </summary>
		/// <param name="initiator"></param>
		/// <param name="functionSignature"></param>
		/// <param name="ownersCount"></param>
		/// <param name="timeout"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static Boolean IsAcceptedBySomeOwners(Byte[] initiator, String functionSignature, Byte ownersCount, UInt32 timeout,
			params Object[] args)
		{
			if (!IsOwner(initiator))
				return false;

			// Convert and concat.
			byte[] mainArray = functionSignature.AsByteArray();
			mainArray.Append(ownersCount);
			mainArray.Concat(((BigInteger)timeout).AsByteArray());
			mainArray.Concat(((BigInteger)GetGenerationOfOwners()).AsByteArray());

			for (int i = 0; i < args.Length; i++)
				mainArray.Concat((byte[])args[0]);

			// Get Sha256.
			byte[] shaMainArray = Sha256(mainArray);

			var value = Storage.Get(Storage.CurrentContext, shaMainArray);
			if (value.Length == 0)
				return false;

			// Check timeout.
			UInt32 firstCallDate = (UInt32)Storage.Get(Storage.CurrentContext, shaMainArray.Concat("FirstCallDate".AsByteArray())).AsBigInteger();

			UInt32 overdueDate = firstCallDate + timeout;

			if (Runtime.Time > overdueDate)
				return false;

			// Get voters count, check it and make a decision.
			byte numberOwners = GetNumberOfOwners();

			byte[] votersMask = Storage.Get(Storage.CurrentContext, shaMainArray.Concat("VotersMask".AsByteArray()));
			byte voted = 0;

			for (byte i = 0; i < numberOwners; i++)
				if (votersMask[i] == 1)
					voted++;

			if (voted < ownersCount)
				return false;

			return true;
		}
	}
}
