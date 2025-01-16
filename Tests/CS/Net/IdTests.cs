using Uccs.Net;
using Uccs.Rdn;
using Xunit;

namespace Uccs.Tests;

public static class IdTests
{
	[Fact]
	public static void Main()
	{
		var e0 = new EntityId(1, 2);
		var e1 = new EntityId(1, 2);
		var e2 = new EntityId(2, 3);

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

		Assert.True(EntityId.TryParse(e0.ToString(), out var e) && e0 == e);
		Assert.False(EntityId.TryParse($"{TableBase.BucketsCountMax}-0", out e0));
		Assert.False(EntityId.TryParse("", out e0));
		Assert.False(EntityId.TryParse("-34", out e0));
		Assert.False(EntityId.TryParse("-sdfdsf", out e0));
		Assert.False(EntityId.TryParse("234324-", out e0));
		Assert.False(EntityId.TryParse("sdfgds-", out e0));
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
