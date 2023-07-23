using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Uccs.Net.Tests
{
	public static class AddressTests
	{
		[Fact]
		public static void HashAnfEqual()
		{
			{
				var p = new Dictionary<ReleaseAddress, object>(){{ReleaseAddress.Parse("a/r/p/v"), null}};
				Assert.True(p.ContainsKey(ReleaseAddress.Parse("a/r/p/v")));
				Assert.False(p.ContainsKey(ReleaseAddress.Parse("a/r/p/vv")));
			}
			{
				var p = new Dictionary<ResourceAddress, object>(){{ResourceAddress.Parse("a/r/p/0.0.0"), null}};
				Assert.True(p.ContainsKey(ResourceAddress.Parse("a/p/r/0.0.0")));
				Assert.False(p.ContainsKey(ResourceAddress.Parse("a/p/r/0.0.1")));
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
