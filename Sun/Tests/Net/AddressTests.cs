using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Xunit;

namespace Uccs.Net.Tests
{
	public static class AddressTests
	{
		static byte[] RandomBytes(int n)
		{
			var b = new byte[n];
			Cryptography.Random.NextBytes(b);
			return b;
		}

 		[Fact]
 		public static void Resource()
 		{
 			var s = new HashSet<ResourceAddress>(){	ResourceAddress.Parse("upv:/a/r"), 
													ResourceAddress.Parse("upc:/aa/rr"),
													ResourceAddress.Parse("upc:zone/aaa/rrr"),
													};

 			Assert.True(s.Contains(ResourceAddress.Parse("upv:/a/r")));
 			Assert.True(s.Contains(ResourceAddress.Parse("upc:/aa/rr")));
 			Assert.True(s.Contains(ResourceAddress.Parse("upc:zone/aaa/rrr")));
 			
			Assert.False(s.Contains(ResourceAddress.Parse("upc:wrong/aaa/rrr")));
 			Assert.False(s.Contains(ResourceAddress.Parse("upc:/a/r")));
 			Assert.False(s.Contains(ResourceAddress.Parse("upv:/aa/rr")));
 		}

 		[Fact]
 		public static void Package()
 		{
 			var p = new HashSet<PackageAddress>(){PackageAddress.Parse("upc:/a/p/r/v")};
 			Assert.True(p.Contains(PackageAddress.Parse("upc:/a/p/r/v")));
 			Assert.False(p.Contains(PackageAddress.Parse("upc:/a/p/r/v-")));
 			Assert.False(p.Contains(PackageAddress.Parse("upc:/a/p/r-/v")));
 			Assert.False(p.Contains(PackageAddress.Parse("upc:/a/p-/r/v")));
 			Assert.False(p.Contains(PackageAddress.Parse("upc:/a-/p/r/v")));
 			Assert.False(p.Contains(PackageAddress.Parse("upv:/a/p/r/v")));
 		}

		[Fact]
		public static void Release()
		{
			var a = new DHAddress { Hash = RandomBytes(32) };
			var ac = new DHAddress{ Hash = a.Hash.ToArray() };
			var b = new DHAddress { Hash = RandomBytes(32) };
			 
			var x = new SDAddress { Resource = ResourceAddress.Parse("upv:/a/p"), Hash = RandomBytes(32), Signature = RandomBytes(65) };
			var xc = new SDAddress{ Resource = ResourceAddress.Parse("upv:/a/p"), Hash = x.Hash.ToArray(), Signature = x.Signature.ToArray() };
			var y = new SDAddress { Resource = ResourceAddress.Parse("upv:/a/p"), Hash = RandomBytes(32), Signature = RandomBytes(65) };

			Assert.True(a == ac && a != b &&
						x == xc && x != y &&
						a != x && ac != xc);

			var l = new List<ReleaseAddress> {a, x};

			Assert.True(l.Contains(a));
			Assert.True(l.Contains(ac));
			Assert.False(l.Contains(b));

			Assert.True(l.Contains(x));
			Assert.True(l.Contains(xc));
			Assert.False(l.Contains(y));

			Assert.True(a == ReleaseAddress.Parse(a.ToString()));

			Assert.True(a == JsonSerializer.Deserialize<ReleaseAddress>(JsonSerializer.Serialize((ReleaseAddress)a, JsonApiClient.Options), JsonApiClient.Options));
			Assert.True(x == JsonSerializer.Deserialize<ReleaseAddress>(JsonSerializer.Serialize((ReleaseAddress)x, JsonApiClient.Options), JsonApiClient.Options));
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
