using System;
using System.Linq;
using Uccs.Net;
using Uccs.Rdn;
using Xunit;

namespace Uccs.Tests;

public static class IdTests
{
	[Fact]
	public static void Main()
	{
		EntityId e0 = new(1, 2);
		EntityId e1 = new(1, 2);
		EntityId e2 = new(2, 3);

		BaseId r0 = new ResourceId(1, 2, 4);
		BaseId r1 = new ResourceId(1, 2, 4);
		BaseId r2 = new ResourceId(1, 2, 5);
		BaseId r3 = new ResourceId(1, 3, 6);

		Assert.True(e0 == e1);
		Assert.True(e0 != e2);

		Assert.True(r0 == r1);
		Assert.True(r0 != r2);
		Assert.True(r1 != r2);

		Assert.True(e0 == r0);
		Assert.True(e2 != r0);
		Assert.True(e0 != r3);

		BaseId[] a = [r3, r2, r1, r0];
		
		a = a.Order().ToArray();

		Assert.True(a.SequenceEqual([r0, r1, r2, r3]));
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
