using System;
using Uccs.Net;
using Uccs.Rdn;
using Uocs;
using Xunit;

namespace Uccs.Tests
{
	public static class LogTests
	{
		[Fact]
		public static void Main()
		{
			var l = new Log();

			new FileLog(l, "test", G.Dev.Tmp);

			for(int i=0; i<1_000_000; i++)
			{
				l.Report($"{i} - {RdnZone.Local.Genesis}");
			}
		}

		//[Theory]
		//[MemberData(nameof(FromWei_TestData))]
		//public static void FromWei(BigInteger wei, Coin expected)
		//{
		//	Coin actual = Coin.FromWei(wei);
		//	Assert.Equal(expected, actual);
		//}

		//public static IEnumerable<object[]> FromWei_TestData()
		//{
		//	yield return new object[] { 1_002, new Coin() };
		//	yield return new object[] { 1_002, new Coin() };
		//	yield return new object[] { 1_002, new Coin() };
		//}

		//[Fact]
		//public static void Ctor_Int()
		//{
		//}

		//[Fact]
		//public static void Ctor_Decimal()
		//{
		//}

		//[Fact]
		//public static void Ctor_Double()
		//{
		//}
	}
}
