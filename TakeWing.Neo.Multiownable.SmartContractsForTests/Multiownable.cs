using System;
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
		/// Call Sha256 by OpCode.
		/// </summary>
		/// <param name="byteArray">Byte array for convert to Sha256 hash.</param>
		/// <returns>Return SHA256 hash.</returns>
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
			key = key.Concat(((BigInteger)index).AsByteArray());

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
			Byte ownersCount = GetNumberOfOwners();

			Byte[][] result = new byte[ownersCount][];
			for (byte i = 0; i < ownersCount; i++)
				result[i] = GetOwnerByIndex((byte)(i + 1));

			return result;
		}

		/// <summary>
		/// Check if public key in owners list.
		/// </summary>
		/// <param name="publicKey">Public key</param>
		/// <returns>True if public key in owners list, false else</returns>
		public static Boolean IsOwner(Byte[] publicKey)
		{
			byte ownerIndex = GetIndexByOwner(publicKey);
			if (ownerIndex == 0)
				return false;

			return true;
		}

		/// <summary>
		/// Transfer ownership to new owners list.
		/// </summary>
		/// <param name="initiator">Initiator's public key</param>
		/// <param name="newOwners">Array of public keys</param>
		/// <returns>Return true after successful transfer, else false</returns>
		public static Boolean TransferOwnership(Byte[] initiator, Byte[][] newOwners)
		{
			UInt64 generationOfOwners = GetGenerationOfOwners();
			if (generationOfOwners > 0)
			{
				byte ownersCount = GetNumberOfOwners();
				if (!IsAcceptedBySomeOwners(initiator, "Boolean TransferOwnership(Byte[], Byte[][])", (byte)((ownersCount / 2) + 1), 1200, newOwners))
					return false;

				// Clear current list of owners.
				for (byte i = 1; i <= ownersCount; i++)
				{
					var keyForOwners = "Owners".AsByteArray();
					var keyForIndexes = "IndexesOfOwners".AsByteArray();

					keyForOwners = keyForOwners.Concat(((BigInteger)i).AsByteArray());
					keyForIndexes = keyForIndexes.Concat(Storage.Get(Storage.CurrentContext, keyForOwners));

					Storage.Delete(Storage.CurrentContext, keyForOwners);
					Storage.Delete(Storage.CurrentContext, keyForIndexes);
				}
			}

			// Set new list of owners.
			for (byte i = 1; i <= newOwners.Length; i++)
			{
				var keyForOwners = "Owners".AsByteArray();
				var keyForIndexes = "IndexesOfOwners".AsByteArray();

				keyForOwners = keyForOwners.Concat(((BigInteger)i).AsByteArray());
				keyForIndexes = keyForIndexes.Concat(newOwners[i - 1]);

				Storage.Put(Storage.CurrentContext, keyForOwners, newOwners[i - 1]);
				Storage.Put(Storage.CurrentContext, keyForIndexes, i);
			}

			// Change generation.
			generationOfOwners++;
			Storage.Put(Storage.CurrentContext, "GenerationOfOwners", generationOfOwners);

			// Change NumberOfOwners
			Storage.Put(Storage.CurrentContext, "NumberOfOwners", newOwners.Length);

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
			// If initiator not Invoker or owner, return false.
			if (!IsOwner(initiator) || !Runtime.CheckWitness(initiator))
				return false;

			// Convert and concat to one array all args.
			byte[] mainArray = functionSignature.AsByteArray();
			mainArray.Concat(((BigInteger)ownersCount).AsByteArray());
			mainArray.Concat(((BigInteger)timeout).AsByteArray());
			mainArray.Concat(((BigInteger)GetGenerationOfOwners()).AsByteArray());

			for (int i = 0; i < args.Length; i++)
				mainArray.Concat((byte[])args[0]);

			// Get Sha256 hash from array.
			byte[] shaMainArray = Sha256(mainArray);

			// Get voters mask.
			Byte totalVoted = Storage.Get(Storage.CurrentContext, shaMainArray.Concat("TotalVoted".AsByteArray()))[0];
			Byte[] votersMask = Storage.Get(Storage.CurrentContext, shaMainArray.Concat("VotersMask".AsByteArray()));
			Byte ownerIndex = GetIndexByOwner(initiator);

			// If that function is called first time, set timeout.
			if (totalVoted == 0)
			{
				int numberOfOwners = GetNumberOfOwners();

				for (int i = 0; i <= numberOfOwners; i++)
				{
					votersMask = votersMask.Concat(new byte[1] { 0 });
					Runtime.Notify(votersMask[i]);
				}

				Runtime.Notify(votersMask.Length);

				Storage.Put(Storage.CurrentContext, shaMainArray.Concat("FirstCallDate".AsByteArray()), Runtime.Time);
			}

			// Check timeout and return false, if time overdue.
			UInt32 firstCallDate = (UInt32)Storage.Get(Storage.CurrentContext, shaMainArray.Concat("FirstCallDate".AsByteArray())).AsBigInteger();
			UInt32 overdueDate = firstCallDate + timeout;

			if (Runtime.Time > overdueDate)
				return false;

			// Check voters and return true or do vote.
			if (votersMask[ownerIndex] != 1)
			{
				totalVoted = (byte)(totalVoted + 1);

				byte[] newVotersMask = new byte[] { };

				for (int i = 0; i < votersMask.Length; i++)
				{
					if (ownerIndex == i)
					{
						newVotersMask = newVotersMask.Concat(new byte[1] { 1 });
					}
					else
					{
						if (votersMask[i] == 1)
						{
							newVotersMask = newVotersMask.Concat(new byte[1] { 1 });
						}
						else
						{
							newVotersMask = newVotersMask.Concat(new byte[1] { 0 });
						}
					}

					Runtime.Notify(newVotersMask[i]);
				}

				Storage.Put(Storage.CurrentContext, shaMainArray.Concat("TotalVoted".AsByteArray()), totalVoted);
				Storage.Put(Storage.CurrentContext, shaMainArray.Concat("VotersMask".AsByteArray()), newVotersMask);

				Runtime.Notify("Voted", initiator);
			}
			else
			{
				Runtime.Notify("This initiator already voted", initiator);
			}

			if (totalVoted == ownersCount)
			{
				Storage.Delete(Storage.CurrentContext, shaMainArray.Concat("TotalVoted".AsByteArray()));
				Storage.Delete(Storage.CurrentContext, shaMainArray.Concat("VotersMask".AsByteArray()));
				Storage.Delete(Storage.CurrentContext, shaMainArray.Concat("FirstCallDate".AsByteArray()));

				return true;
			}
			else
				return false;
		}
	}
}
