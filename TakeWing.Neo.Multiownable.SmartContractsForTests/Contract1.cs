using System;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;


namespace TakeWing.Neo.Multiownable.SmartContractsForTests
{
	public class Contract1 : SmartContract
	{
		public static Object Main(String operation, params Object[] args)
		{
			Runtime.Notify("Invoke.", operation, args);

			if (operation == "TransferOwnership")
			{
				byte[] initiator = (byte[])args[0];
				int countOwners = (int)((byte[])args[1]).AsBigInteger();

				byte[][] newOwners = new byte[countOwners][];
				for (int i = 0, j = 2; i < countOwners; i++, j++)
					newOwners[i] = (byte[])args[j];

				Runtime.Notify("TransferOwnership", initiator, newOwners);
				return Multiownable.TransferOwnership(initiator, newOwners);
			}
			else if (operation == "Call")
			{
				byte[] initiator = (byte[])args[0];
				string functionSignature = ((byte[])args[1]).AsString();
				Byte ownersCount = (byte)args[2];
				UInt32 timeout = (UInt32)((byte[])args[3]).AsBigInteger();
				int argsCount = (int)((byte[])args[4]).AsBigInteger();

				object[] argsForMultiownable = new object[argsCount];
				for (int i = 0, j = 5; i < argsCount; i++, j++)
					argsForMultiownable[i] = (byte[])args[j];

				if (Multiownable.IsAcceptedBySomeOwners(initiator, functionSignature, ownersCount, timeout,
					argsForMultiownable))
				{
					Runtime.Notify("Call function success.");
					return true;
				}
				else
				{
					Runtime.Notify("Vote success.");
					return false;
				}
			}
			else if (operation == "GetNumberOfOwners")
			{
				return Multiownable.GetNumberOfOwners();
			}
			else if (operation == "GetOwnerByIndex")
			{
				byte index = (byte)args[0];

				return Multiownable.GetOwnerByIndex(index);
			}
			else if (operation == "GetIndexByOwner")
			{
				byte[] owner = (byte[])args[0];

				return Multiownable.GetIndexByOwner(owner);
			}
			else if (operation == "GetGenerationOfOwners")
			{
				return Multiownable.GetGenerationOfOwners();
			}
			else if (operation == "GetAllOwners")
			{
				return Multiownable.GetAllOwners();
			}
			else if (operation == "IsOwner")
			{
				byte[] owner = (byte[]) args[0];

				return Multiownable.IsOwner(owner);
			}
			else
			{
				Runtime.Notify("Unknown operation.");
				return false;
			}
		}
	}
}
