using System;
using System.Numerics;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using TakeWing.Neo.Multiownable;


namespace TakeWing.Neo.Multiownable.SmartContractsForTests
{
	public class Contract1 : SmartContract
	{
		public static void Main()
		{
			var a = Multiownable.GetOwnerByIndex(1);
			Storage.Put(Storage.CurrentContext, "Hello", "World");
		}
	}
}
