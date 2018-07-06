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
				byte[] initiator = (byte[]) args[0];
				BigInteger countOwners = ((byte[]) args[1]).AsBigInteger();

				byte[][] newOwners = new byte[][] { };
				for (int i = 0; i < countOwners; i++)
				{
					newOwners[i] = (byte[]) args[i + 1];
				}

				Runtime.Notify("TransferOwnership", initiator, newOwners);
				return Multiownable.TransferOwnership(initiator, newOwners);
			}
			else
			{
				Runtime.Notify("Unknown operation.");
				return false;
			}
		}
	}
}
