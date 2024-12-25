using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public static class Serialization
{
	[Fact]
	public static void Main()
	{
		var a = new Dictionary<string, int>() {{"first", 1}, {"second", 2}};
		var b = new Dictionary<string, int>() {{"first", 1}};

		Assert.False(a.SequenceEqual(b));

		var s = new MemoryStream();
		var w = new BinaryWriter(s);
		
		BinarySerializator.Serialize(w, a, t => 0);

		var r = new BinaryReader(s);
		s.Seek(0, SeekOrigin.Begin);
		Assert.True(a.SequenceEqual(BinarySerializator.Deserialize<Dictionary<string, int>>(r, null)));


		var od = new OrderedDictionary<string, int>() {{"first", 1}, {"second", 2}};

		s = new MemoryStream();
		w = new BinaryWriter(s);
		
		BinarySerializator.Serialize(w, a, t => 0);

		r = new BinaryReader(s);
		s.Seek(0, SeekOrigin.Begin);
		Assert.True(od.SequenceEqual(BinarySerializator.Deserialize<Dictionary<string, int>>(r, null)));


		var sd = new SortedDictionary<string, int>() {{"first", 1}, {"second", 2}};

		s = new MemoryStream();
		w = new BinaryWriter(s);
		
		BinarySerializator.Serialize(w, a, t => 0);

		r = new BinaryReader(s);
		s.Seek(0, SeekOrigin.Begin);
		Assert.True(sd.SequenceEqual(BinarySerializator.Deserialize<Dictionary<string, int>>(r, null)));
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
