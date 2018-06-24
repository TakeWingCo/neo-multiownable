using System;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;


namespace TakeWing.Neo.Multiownable.SmartContractsForTests
{
	public class Contract1 : SmartContract
	{
		public static void Main()
		{
			Storage.Put(Storage.CurrentContext, "Hello", "World");
		}
	}
}
