using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace UC.Net.Tests
{
	public static class AddressTests
	{
		[Fact]
		public static void HashAnfEqual()
		{
			{
				var p = new Dictionary<RealizationAddress, object>(){{RealizationAddress.Parse("a/p/p"), null}};
				Assert.True(p.ContainsKey(RealizationAddress.Parse("a/p/p")));
				Assert.False(p.ContainsKey(RealizationAddress.Parse("a/p/pp")));
			}
			{
				var p = new Dictionary<ReleaseAddress, object>(){{ReleaseAddress.Parse("a/p/p/0.0.0"), null}};
				Assert.True(p.ContainsKey(ReleaseAddress.Parse("a/p/p/0.0.0")));
				Assert.False(p.ContainsKey(ReleaseAddress.Parse("a/p/p/0.0.1")));
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
